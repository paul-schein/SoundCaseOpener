import {Component, inject, signal, WritableSignal} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {SoundFileService} from '../../../core/services/sound-file-service';
import {SnackbarService} from '../../../core/services/snackbar-service';

@Component({
  selector: 'app-file-upload',
  imports: [
    MatButton
  ],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.scss'
})
export class FileUpload {
  protected soundFileService: SoundFileService = inject(SoundFileService);
  protected snackBarService: SnackbarService = inject(SnackbarService);
  protected files: WritableSignal<File[]> = signal([]);
  protected isDragOver: WritableSignal<boolean> = signal(false);

  public onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver.set(true);
  }

  public onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver.set(false);
  }

  public onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver.set(false);
    if (event.dataTransfer?.files) {
      this.addFiles(event.dataTransfer.files);
    }
  }

  public onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.addFiles(input.files);
    }
  }

  public clearFiles(): void {
    this.files.set([]);
  }

  public async pushFiles() {
    await this.soundFileService.uploadFiles(this.files());
    this.clearFiles();
    this.snackBarService.show("Files successfully uploaded!")
  }

  private addFiles(fileList: FileList) {
    this.files.update(files => {
      const newFiles = [...files];
      for (let i = 0; i < fileList.length; i++) {
        newFiles.push(fileList.item(i)!);
      }
      return newFiles;
    });
  }
}
