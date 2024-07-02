
var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("priceLowToHigh")) {
        loadDataTable("priceLowToHigh");
    }
    else {
        if (url.includes("priceHighToLow")) {
            loadDataTable("priceHighToLow");
        }
        else {
            if (url.includes("nameAscending")) {
                loadDataTable("nameAscending");
            }
            else {
                if (url.includes("nameDescending")) {
                    loadDataTable("nameDescending");
                }
                else {
                    loadDataTable("all");
                }
            }
        }
    }

});

function loadDataTable(sortOrder) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/Product/getall?sortOrder=' + sortOrder },
        "columns": [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'price', "width": "10%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/admin/Product/details?sortOrder=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i></a>
                    
                    </div>`
                },
                "width": "10%"
            }
        ]
    });
}