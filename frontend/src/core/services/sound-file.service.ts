import {Injectable} from '@angular/core';
import {ServiceBase} from './service-base';
import { HttpStatusCode } from '@angular/common/http';
import { z } from 'zod';
import {lastValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SoundFileService extends ServiceBase {
  protected override get controller(): string {
    return 'sound-files';
  }

  private async uploadSingleFile(file: File): Promise<UploadResult> {
    const parseResult = soundFileSchema.safeParse(file);

    if (!parseResult.success) {
      return new UploadResult(HttpStatusCode.BadRequest);
    }

    const validFile = parseResult.data;
    const formData = new FormData();
    formData.append('file', validFile);

    const url = this.buildUrl(`${validFile.name}`);

    try {
      const response = await lastValueFrom(this.http.post(url, formData, {
        observe: 'response'
      }));
      return new UploadResult(response.status);
    } catch (error: any) {
      return new UploadResult(error.status || HttpStatusCode.InternalServerError);
    }
  }

  public async uploadFiles(files: File[]): Promise<{
    success: boolean,
    results: UploadResult[],
    failedFiles: string[]
  }> {
    const parseResult = z.array(soundFileSchema).safeParse(files);

    if (!parseResult.success) {
      return {
        success: false,
        results: [new UploadResult(HttpStatusCode.BadRequest)],
        failedFiles: Array.from(files).map(f => f.name)
      }
    }

    const validFiles = parseResult.data;
    const results: UploadResult[] = [];
    const failedFiles: string[] = [];

    for (const file of validFiles) {
      const result = await this.uploadSingleFile(file);
      results.push(result);

      if (!result.success) {
        failedFiles.push(file.name);
      }
    }

    return {
      success: results.every(r => r.success),
      results,
      failedFiles
    };
  }
}

const fileSchema = z.custom<File>((file) => file instanceof File, {
  message: 'Must be a valid File object',
});

const soundFileSchema = fileSchema.refine(
  (file) => file.size <= 24 * 1024 * 1024,
  { message: 'Audio file size must be less than 24MB' }
).refine(
  (file) => [
    'audio/mpeg', 'audio/mp3', 'audio/wav',
    'audio/ogg', 'audio/x-m4a', 'audio/aac',
    'audio/flac'
  ].includes(file.type),
  { message: 'File must be a valid audio format (MP3, WAV, OGG, M4A, AAC, FLAC)' }
);

export class UploadResult {
  constructor(private readonly statusCode: number) {
  }

  public get success(): boolean {
    return this.statusCode === HttpStatusCode.Created;
  }

  public get fileAlreadyExists(): boolean {
    return this.statusCode === HttpStatusCode.Conflict;
  }
}
