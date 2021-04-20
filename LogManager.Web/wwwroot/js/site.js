let progressInterval, dotInterval;
const progressIntervalLength = 3000;
const dotIntervalLength = 1000;

function ShowProgress(fileName) {

    $("#processed-file").html(fileName)
    $("#process-percent").html("0%")

    progressInterval = setInterval(() => {
        $.post("/Home/GetLoadProgress", undefined, (data) => {
            console.log(data)
            $("#process-percent").html(parseInt(data) + "%")

            if (parseInt(data) === 100) {
                clearInterval(progressInterval)
                clearInterval(dotInterval)
            }
        })  
    }, progressIntervalLength)

    dotInterval = setInterval(() => {
        $("#process-percent").append('.')
    }, dotIntervalLength)

}

$(document).ready(function () {
    $('.file-table th').on("click", FileSortClick)
    $('#file-search').on("click", FileSearchClick)

    $('.ip-table th').on("click", IpSortClick)
    $('#ip-search').on("click", IpSearchClick)

})

let sortFieldNames = new Map([
    ['file-path', 'Path'],
    ['file-title', 'Title'],
    ['file-size', 'Size'],
    ['ip-address', 'AddressAsLong'],
    ['ip-owner-name', 'OwnerName'],
    ['log-date', 'Date'],
    ['log-method', 'Method'],
    ['log-ip-address', 'Ip.AddressStr'],
    ['log-ip-owner', 'Ip.OwnerName'],
    ['log-amount', 'Amount'],
    ['log-status', 'StatusCode'],
])

let requestInfo = {}

function SortSetup(buttonElement) {
    requestInfo.id = buttonElement.attr('id')
    requestInfo.fieldToSort = sortFieldNames.get(requestInfo.id)
    requestInfo.isDescending = $('#sort-descending-input').val() == 'True'
    requestInfo.oldFieldToSort = $('#sort-field-input').val()

    if (requestInfo.fieldToSort == requestInfo.oldFieldToSort) {
        requestInfo.isDescending = !requestInfo.isDescending
    }

    requestInfo.pageSize = $('#page-size-input').val()
    requestInfo.searchText = $('#search-input').val()
}

function FileSortClick() {
    SortSetup($(this))

    console.log(requestInfo)

    $.get(`/Home/FileAjax?page=1&pageSize=${requestInfo.pageSize}&sortField=${requestInfo.fieldToSort}&isDescending=${requestInfo.isDescending}&searchText=${requestInfo.searchText}`,
        undefined,
        (data) => {
            $('.file-area').html(data)
            $('#' + requestInfo.id).append(requestInfo.isDescending ? '&#9660;' : '&#9650;')
            $('.file-table th').on("click", FileSortClick)
            $('#file-search').on("click", FileSearchClick)
        })
}

function FileSearchClick() {

    let pageSize = $('#page-size-input').val()
    let searchText = $('#search-input').val()

    $.get(`/Home/FileAjax?page=1&pageSize=${pageSize}&searchText=${searchText}`,
        undefined,
        (data) => {
            $('.file-area').html(data)
            $('.file-table th').on("click", FileSortClick)
            $('#file-search').on("click", FileSearchClick)
        })
}

function IpSortClick() {
    SortSetup($(this))

    $.get(`/Home/IpAjax?page=1&pageSize=${requestInfo.pageSize}&sortField=${requestInfo.fieldToSort}&isDescending=${requestInfo.isDescending}&searchText=${requestInfo.searchText}`,
        undefined,
        (data) => {
            $('.ip-area').html(data)
            $('#' + requestInfo.id).append(requestInfo.isDescending ? '&#9660;' : '&#9650;')
            $('.ip-table th').on("click", IpSortClick)
            $('#ip-search').on("click", IpSearchClick)
        })
}

function IpSearchClick() {

    let pageSize = $('#page-size-input').val()
    let searchText = $('#search-input').val()

    $.get(`/Home/IpAjax?page=1&pageSize=${pageSize}&searchText=${searchText}`,
        undefined,
        (data) => {
            $('.ip-area').html(data)
            $('.ip-table th').on("click", IpSortClick)
            $('#ip-search').on("click", IpSearchClick)
        })
}