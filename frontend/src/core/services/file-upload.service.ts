import {Injectable} from '@angular/core';
import {ServiceBase} from './service-base';
import { z } from 'zod';

@Injectable({
  providedIn: 'root',
})
export class FileUploadService extends ServiceBase {
  protected override get controller(): string {
    return 'file-upload';
  }

  public async uploadFiles() {

  }
}

const fileSchema = z.custom<File>((file) => file instanceof File, {
  message: 'Must be a valid File object',
});

const audioFileSchema = fileSchema.refine(
  (file) => file.size <= 24 * 1024 * 1024, // 15MB max
  { message: 'Audio file size must be less than 24MB' }
).refine(
  (file) => [
    'audio/mpeg', 'audio/mp3', 'audio/wav',
    'audio/ogg', 'audio/x-m4a', 'audio/aac',
    'audio/flac'
  ].includes(file.type),
  { message: 'File must be a valid audio format (MP3, WAV, OGG, M4A, AAC, FLAC)' }
);
