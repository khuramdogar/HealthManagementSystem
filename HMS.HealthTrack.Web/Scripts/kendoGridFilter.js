//http://jsfiddle.net/randombw/27hTK/
function customTextFilter(grid, field, value) {
   var currentFilterObject = grid.dataSource.filter();
   var currentFilters = currentFilterObject ? currentFilterObject.filters : [];
   if (currentFilters && currentFilters.length > 0) {
      for (var i = 0; i < currentFilters.length; i++) {
         if (currentFilters[i].field == field) {
            currentFilters.splice(i, 1);
            break;
         }
      }
   }

   if (value != '') {
      currentFilters.push({
         field: field,
         operator: "contains",
         value: value
      });
   }
   grid.dataSource.filter({
      logic: "and",
      filters: currentFilters
   });
}

function customExactTextFilter(grid, field, value) {
   var currentFilterObject = grid.dataSource.filter();
   var currentFilters = currentFilterObject ? currentFilterObject.filters : [];
   if (currentFilters && currentFilters.length > 0) {
      for (var i = 0; i < currentFilters.length; i++) {
         if (currentFilters[i].field == field) {
            currentFilters.splice(i, 1);
            break;
         }
      }
   }

   if (value != '') {
      currentFilters.push({
         field: field,
         operator: "eq",
         value: value
      });
   }
   grid.dataSource.filter({
      logic: "and",
      filters: currentFilters
   });
}

// http://www.telerik.com/forums/adding-filters-to-grid-s-source
function UpdateSearchFilters(filters, field, operator, value, logic) {
   if (value == "") {
      return RemoveSearchFilter(filters, field);
   }
   var newFilter = { field: field, operator: operator, value: value.trim() };

   if (filters.length == 0) {
      filters.push(newFilter);
   }
   else {
      var isNew = true;
      var index = 0;

      for (index = 0; index < filters.length; index++) {
         if (filters[index].field == field && filters[index].operator == operator) {
            isNew = false;
            break;
         }
      }

      if (isNew) {
         filters.push(newFilter);
      }
      else {
         filters[index] = newFilter;
      }
   }
   return filters;
}

function RemoveSearchFilter(filters, field) {
   if (filters.length == 0)
      return filters;

   for (var ii = 0; ii < filters.length; ii++) {
      if (filters[ii].field == null) {
         RemoveSearchFilter(filters[ii].filters, field);
         filters.splice(ii, 1);
      } else {
         if (filters[ii].field == field) {
            filters.splice(ii, 1);
            ii--;
         }
      }
   }
   return filters;
}

function GetFilters(grid) {
   if (grid.dataSource._filter == null)
      return [];

   return grid.dataSource._filter.filters;
}

function ClearFilters(grid) {
   grid.dataSource.filter([]);
}

function InitFilterControls() {
   $('.filter-control').on("keydown", function(keyEvent) {
      var code = (keyEvent.keyCode ? keyEvent.keyCode : keyEvent.which);
      if (code == 13) {
         $('#applyFilter').trigger('click');
         keyEvent.preventDefault();
      }
   });
}