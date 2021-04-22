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

    $('.log-table th').on("click", LogSortClick)
    $('#log-search').on("click", LogSearchClick)
})

let sortFieldNames = new Map([
    ['file-path', 'Path'],
    ['file-title', 'Title'],
    ['file-size', 'Size'],
    ['ip-address', 'Address'],
    ['ip-owner-name', 'OwnerName'],
    ['log-date', 'Date'],
    ['log-method', 'Method'],
    ['log-file-path', 'FileInfo.Path'],
    ['log-file-title', 'FileInfo.Title'],
    ['log-ip-address', 'IpInfo.Address'],
    ['log-ip-owner-name', 'IpInfo.OwnerName'],
    ['log-amount', 'Amount'],
    ['log-status', 'StatusCode'],
])

let requestInfo = {}

function SortSetup(buttonElement) {
    requestInfo.id = buttonElement.attr('id')

    if (!sortFieldNames.has(requestInfo.id))
        return false

    requestInfo.fieldToSort = sortFieldNames.get(requestInfo.id)
    requestInfo.isDescending = $('#sort-descending-input').val() == 'True'
    requestInfo.oldFieldToSort = $('#sort-field-input').val()

    if (requestInfo.fieldToSort == requestInfo.oldFieldToSort) {
        requestInfo.isDescending = !requestInfo.isDescending
    }

    requestInfo.pageSize = $('#page-size-input').val()
    requestInfo.searchText = $('#search-input').val()

    return true;
}

function FileSortClick() {
    var result = SortSetup($(this))

    if (!result)
        return

    $.get(`/Home/FileAjax?page=1&pageSize=${requestInfo.pageSize}&sortField=${requestInfo.fieldToSort}&isDescending=${requestInfo.isDescending}&searchText=${requestInfo.searchText}`,
        undefined,
        FileResponseHandler)
}

function FileSearchClick() {

    let pageSize = $('#page-size-input').val()
    let searchText = $('#search-input').val()

    $.get(`/Home/FileAjax?page=1&pageSize=${pageSize}&searchText=${searchText}`,
        undefined,
        FileResponseHandler)
}

function FileResponseHandler(data) {
    $('.file-area').html(data)
    $('.file-table th').on("click", FileSortClick)
    $('#file-search').on("click", FileSearchClick)
}

function IpSortClick() {
    var result = SortSetup($(this))

    if (!result)
        return

    $.get(`/Home/IpAjax?page=1&pageSize=${requestInfo.pageSize}&sortField=${requestInfo.fieldToSort}&isDescending=${requestInfo.isDescending}&searchText=${requestInfo.searchText}`,
        undefined,
        IpResponseHandler)
}

function IpSearchClick() {

    let pageSize = $('#page-size-input').val()
    let searchText = $('#search-input').val()

    $.get(`/Home/IpAjax?page=1&pageSize=${pageSize}&searchText=${searchText}`,
        undefined,
        IpResponseHandler)
}

function IpResponseHandler(data) {
    $('.ip-area').html(data)
    $('.ip-table th').on("click", IpSortClick)
    $('#ip-search').on("click", IpSearchClick)
}

function LogSortClick() {
    var result = SortSetup($(this))

    if (!result)
        return

    $.get(`/Home/LogEntryAjax?page=1&pageSize=${requestInfo.pageSize}&sortField=${requestInfo.fieldToSort}&isDescending=${requestInfo.isDescending}&searchText=${requestInfo.searchText}`,
        undefined,
        LogResponseHandler)
}

function LogSearchClick() {

    let pageSize = $('#page-size-input').val()
    let searchText = $('#search-input').val()

    $.get(`/Home/LogEntryAjax?page=1&pageSize=${pageSize}&searchText=${searchText}`,
        undefined,
        LogResponseHandler)
}

function LogResponseHandler(data) {
    $('.log-area').html(data)
    $('.log-table th').on("click", LogSortClick)
    $('#log-search').on("click", LogSearchClick)
}
