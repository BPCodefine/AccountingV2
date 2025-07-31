import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { environment } from '../../Environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private http: HttpClient) {}

getUsername(): Observable<string> {
  console.log('getUsername() called');

  return this.http.get<{ userName: string }>(environment.apiUrl + '/api/username', {withCredentials: true}).pipe(
    map(response => {
      console.log('Response received:', response);
      console.log('Raw response:', JSON.stringify(response));
      const result = response?.userName?.trim() ? response.userName : 'no user found';
      console.log('Mapped username:', result);
      return result;
    }),
    catchError((err) => {
      console.warn('Caught HTTP error:', err);
      if (environment.useDevFallbackUser) {
          return of('bp');
        } else {
        return of('xxxx');
        }
    }),
    tap(final => console.log('Final emitted value:', final))
  );
}

  getEnabledCompanies(userName: string): Observable<string[]> {
    return this.http.get<string[]>(environment.apiUrl + '/api/GetEnabledComps' +
      '?username=' + encodeURIComponent(userName)).pipe(
      catchError(err => {
        console.warn('Could not retrieve enabled companies:', userName + ' ' + err);
        return of([]);
      })
    );
  }
}
