import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import Todo from './Todo';

@Injectable()
export default class ApiService {
  public API = 'http://localhost:57645/home';
  public URL = `${this.API}/todoItems`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<Array<Todo>> {
    return this.http.get<Array<Todo>>(this.URL);
  }
}
