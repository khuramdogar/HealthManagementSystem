import { Component, OnInit } from '@angular/core';

import Todo from '../shared/Todo';
import ApiService from '../shared/api.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit  {
  todoRecords: Array<Todo>;

  constructor(private apiService: ApiService) {
  }

  ngOnInit() {
    this.apiService.getAll().subscribe(data => {
      this.todoRecords = data;
    });
  }
}
