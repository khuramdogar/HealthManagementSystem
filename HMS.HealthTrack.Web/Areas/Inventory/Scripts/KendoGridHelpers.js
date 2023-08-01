function getDataItemFromGrid(event, grid) {
   var row = $(event.currentTarget).closest('tr');
   return $('#' + grid).data('kendoGrid').dataItem(row);
}