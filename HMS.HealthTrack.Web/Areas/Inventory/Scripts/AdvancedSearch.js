// When adding new fields to the advanced search screen, they must be added to the methods below.

// Categories are handled separately and passed through to the controller as a parameter
function filterGridWithAdvancedSearch(grid) {
   var filters = GetFilters(grid);
   filters = UpdateSearchFilters(filters, "Description", "contains", $('#psDescription').val(), "and");
   filters = UpdateSearchFilters(filters, "UPN", "contains", $('#psUPN').val(), "and");
   filters = UpdateSearchFilters(filters, "SPC", "contains", $('#psSPC').val(), "and");
   filters = UpdateSearchFilters(filters, "PrimarySupplier", "eq", $('#psSupplier').val(), "and");
   filters = UpdateSearchFilters(filters, "Manufacturer", "contains", $('#psManufacturer').val(), "and");
   filters = UpdateSearchFilters(filters, "RebateCode", "contains", $('#psRebateCode').val(), "and");
   
   filters = applyNullableBoolFilter(filters, "InStock");
   filters = applyNullableBoolFilter(filters, "Unclassified");
   filters = applyNullableBoolFilter(filters, "IsConsignment");
   filters = applyNullableBoolFilter(filters, "HasHadStockTake");
   filters = applyNullableBoolFilter(filters, "PendingConsumedProducts");
   filters = applyNullableBoolFilter(filters, "InError");
   filters = applyNullableBoolFilter(filters, "ManageStock");
   grid.dataSource.filter(filters);
}


function ClearAdvancedSearchControls() {
   $('#psDescription').val('');
   $('#psUPN').val('');
   $('#psSPC').val('');
   $('#psManufacturer').val('');
   $('#psRebateCode').val('');

   $('#psSupplier').data('kendoDropDownList').value('');

   $('#psSelectedCategories').data('kendoMultiSelect').value([]);
   $('#psStatus').data('kendoMultiSelect').value([]);

   $('#psInStock').data('kendoDropDownList').value('');
   $('#psIsConsignment').data('kendoDropDownList').value('');
   $('#psUnclassified').data('kendoDropDownList').value('');
   $("#psHasHadStockTake").data('kendoDropDownList').value('');
   $("#psPendingConsumedProducts").data('kendoDropDownList').value('');
   $("#psInError").data('kendoDropDownList').value('');
   $("#psManageStock").data('kendoDropDownList').value('');

}

// ----------------------------------------------------------------------------------------------------------------------------------------

function createAdvancedSearch() {
   $('#advancedSearchWindow').kendoWindow({
      width: '750',
      title: 'Advanced product search',
      content: '/Inventory/ProductSearch/ProductSearch',
      visible: false,
      modal: true,
      refresh: function (e) {
         advancedSearchButtons();
         e.sender.center();
      },
      open:function(e) {
         
      }
   });
}

function createAdvancedSearchWithGrid() {
   $('#advancedSearchWindow').kendoWindow({
      width: '745px',
      title: 'Advanced product search',
      content: '/Inventory/ProductSearch/ProductSearchWithGrid',
      visible: false,
      modal: true,
      refresh: function (e) {
         onAdvancedSearchRefresh(e);
         e.sender.center();
      },
      open: function(e) {
         ClearAdvancedSearchControls();
         var resultsGrid = $('#Results').data('kendoGrid');
         resultsGrid.dataSource._filter = null;
         resultsGrid.dataSource.data([]);
      }
   });
}

function advancedSearchButtons() {
   $('#advancedSearchWindow').keypress(function (ee) {
      if (ee.keyCode == 13) {
         $('#search').trigger('click');
      }
   });

   $('#advancedSearchWindow').on('click', '#psClose', function () {
      $('#advancedSearchWindow').data('kendoWindow').close();
   });

   $('#advancedSearchWindow').on('click', '#psClear', function () {
      ClearAdvancedSearchControls();
   });
}

function onAdvancedSearchRefresh(e) {
   advancedSearchButtons();
   $('#Results').on('dblclick', 'tbody > tr', function () {
      $('#selectProductSearch').trigger('click');
   });

   $('#selectProductSearch').click(function () {
      var grid = $('#Results').data('kendoGrid');
      var selected = grid.dataItem(grid.select());
      if (selected == null) return;
      var productId = parseInt(selected.ProductId);
      $('#ProductId').val(selected.ProductId);
      $('#ProductId').trigger('change');

      var dropDownList = $('#Product').data('kendoDropDownList');
      if (dropDownList != undefined) {
         var dataSource = dropDownList.dataSource;

         var item = getItemFromDataSource(dataSource, productId);
         if (item == null) {
            dataSource.add({ Text: selected.Description, Value: productId });
            item = getItemFromDataSource(dataSource, productId);
         }

         var index = dataSource.indexOf(item);
         dropDownList.select(++index); // account for blank item in list
      }
      e.sender.close();
   });

   $('#cancelProductSearch').click(function () {
      e.sender.close();
   });
}

function openAdvancedSearch() {
   $('#advancedSearchWindow').data('kendoWindow').center().open();
}

// empty grid results
function resultGridDataBound(e) {
   applyGridTooltips();
   var grid = e.sender;
   if (grid.dataSource.total() == 0) {
      var colCount = grid.columns.length;
      $(e.sender.wrapper)
         .find('tbody')
         .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" class="no-data" style="text-align:center" >No results found.</td></tr>');
   }
}

function applyNullableBoolFilter(filters, name) {
   var value = $('#ps' + name).val();
   if (value == '') {
      RemoveSearchFilter(filters, name);
   } else {
      var boolValueString = (value == "Yes").toString();
      filters = UpdateSearchFilters(filters, name, "eq", boolValueString, "and");
   }
   return filters;
}

// disable categories when unclassified is Yes
function onUnclassifiedChange(e) {
   var selectedCategories = $('#psSelectedCategories').data('kendoMultiSelect');
   var browseCategories = $('#psBrowseCategories').data('kendoButton');

   if (e.sender.value() == "Yes") {
      browseCategories.enable(false);
      selectedCategories.enable(false);
   } else {
      browseCategories.enable(true);
      selectedCategories.enable(true);
      selectedCategories.readonly(true);
   }
}

// sends categories as datasource parameter
function productsData() {
   var categoryIds = $('#psSelectedCategories').val();
   if ($('#psUnclassified').length > 0 && $('#psUnclassified').data('kendoDropDownList').value() == "Yes") {
      categoryIds = null;
   }
   
   return {
      categoryIds: categoryIds,
      statuses: $('#psStatus').val(),
      includeDeleted: $('#psIncludeDeleted').val()
   }
}

function bindAdvancedSearchButtonToGrid(grid) {
   $('#advancedSearchWindow').on('click', '#search', function () {
      filterGridWithAdvancedSearch(grid);
      $('#advancedSearchWindow').data('kendoWindow').close();
   });
}