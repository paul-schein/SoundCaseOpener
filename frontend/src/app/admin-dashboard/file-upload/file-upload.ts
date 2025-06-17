import {Component, signal, WritableSignal} from '@angular/core';
import {MatButton} from '@angular/material/button';

@Component({
  selector: 'app-file-upload',
  imports: [
    MatButton
  ],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.scss'
})
export class FileUpload {
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
