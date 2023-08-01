function ignoreConsumption(usedId) {
   $.ajax({
      async: false,
      contentType: 'application/json',
      data: JSON.stringify({
         ConsumptionId: usedId
      }),
      type: 'POST',
      url: '/Inventory/Consumptions/IgnoreConsumption',
      success: function(data) {
         getAlert('success', 'Successfully ignored the consumption for product <i>' + data.Name + '</i>.');
         var grid = $('#grid').data('kendoGrid');
         var dataItem = grid.dataSource.get(usedId);
         grid.dataSource.remove(dataItem);
         grid.dataSource.sync();
      },
   error: function(data) {
      alert('An error occurred. Unable to ignore consumption');
   }
});
}