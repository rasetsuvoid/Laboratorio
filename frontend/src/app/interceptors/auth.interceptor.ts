import { HttpInterceptorFn } from '@angular/common/http';

function readCookie(name: string): string | null {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length < 2) {
    return null;
  }
  return decodeURIComponent(parts.pop()!.split(';').shift()!);
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const csrf = readCookie('XSRF-TOKEN');
  let headers = req.headers;

  if (csrf) {
    headers = headers.set('X-XSRF-TOKEN', csrf);
  }

  return next(req.clone({ headers, withCredentials: true }));
};
