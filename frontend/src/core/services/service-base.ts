import {Directive, inject} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {ConfigService} from './config-service';

@Directive()
export abstract class ServiceBase {
  private readonly configService: ConfigService = inject(ConfigService);
  private static baseUrl: string | null = null;
  protected readonly http: HttpClient = inject(HttpClient);

  protected abstract get controller(): string;

  protected buildUrl(action: string | null, ...queryParams: ((QueryParam | null) | undefined)[]): string {
    if (ServiceBase.baseUrl === null) {
      ServiceBase.baseUrl = `${this.configService.config.backendBaseUrl}/api`;
    }
    let url = `${ServiceBase.baseUrl}/${this.controller}`;
    if (action) {
      url += `/${action}`;
    }
    if (queryParams.length > 0 && queryParams.some(qp => qp != undefined)) {
      let first = true;

      for (const [key, value] of queryParams.filter(qp => qp != null && qp[0] != null).map(p => p as QueryParam)) {
        let prefix = "&";
        if (first) {
          prefix = "?";
          first = false;
        }
        url = url.concat(`${prefix}${key}=${value?.toString()}`);
      }

    }

    return url;
  }
}
export type QueryParam = [string, any];
