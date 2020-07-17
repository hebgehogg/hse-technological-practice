$(document).ready(function () {
    $('.slider-wrapper').slick({
        dots: true,
        autoplay: true,
        autoplaySpeed: 3000
    });
    $('#reg-btn-form').on('click', function() {
        var checkReg = true;
        if ($('#regform-fio').val().length > 90 || $('#regform-fio').val().length < 3 ||
            $('#regform-login').val().length > 90 || $('#regform-fio').val().length < 3 ||
            $('#regform-pass').val().length > 90 || $('#regform-pass').val().length < 3 ||
            !validateLogin($('#regform-login').val())) {
            $('#errorReg').val("Неверный формат ввода, повторите попытку");
            checkReg = false;
        }
        if (!checkReg) {
            $('#errorReg').fadeTo("fast", 1);
            return false;
        }
    })
    $('#login-btn-form').on('click', function () {
        var checkReg = true;
        if ($('#loginform-login').val().length > 90 || $('#loginform-login').val().length < 3 ||
            $('#loginform-pass').val().length > 90 || $('#loginform-pass').val().length < 3 ||
            !validateLogin($('#loginform-login').val())) {
            $('#errorLog').val("Неверный формат ввода, повторите попытку");
            checkReg = false;
        }
        if (!checkReg) {
            $('#errorLog').fadeTo("fast", 1);
            setTimeout(() => { $('#errorLog').fadeTo("fast", 0); }, 5000);
            return false;
        }
    })
    $('#addMny-btn-nav').on('click', function (e) {
        e.preventDefault();
        if ($('.addmny-wrapper').is(":visible")) {
            $('.addmny-wrapper').fadeTo("fast", 0);
            $('.addmny-wrapper').css('display','none');
        }
        else {
            $('.addmny-wrapper').css('display','flex');
            $('.addmny-wrapper').fadeTo("fast", 1);
        }
    })
    $('#addmny-btn').on('click', function () {
        if ($.isNumeric($('#addmny-input').val()) && $('#addmny-input').val() < 4000) {
            return true;
        }
        else {
            $('#addmny-error').fadeTo("fast", 1);
            return false;
        }
    })
    $('#acc-change-login-btn').on('click', function () {
        var checkLoginChange = true;
        if ($('#acc-change-ClientLogin').val().length < 3 ||
            $('#acc-change-ClientLogin').val().length > 90 ||
            !validateLogin($('#acc-change-ClientLogin').val())) {
            checkLoginChange = false;
        }
        if (!checkLoginChange) {
            $('#change-prof-error').fadeTo("fast", 1);
            setTimeout(() => { $('#change-prof-error').fadeTo("fast", 0); }, 5000);
            return false;
        }
    })
    $('#acc-change-pass-btn').on('click', function () {
        var checkLoginChange = true;
        if ($('#acc-change-ClientPassword').val().length < 3 ||
            $('#acc-change-ClientPassword').val().length > 90) {
            checkLoginChange = false;
        }
        if (!checkLoginChange) {
            $('#change-prof-error').fadeTo("fast", 1);
            setTimeout(() => { $('#change-prof-error').fadeTo("fast", 0); }, 5000);
            return false;
        }
    })
    $('#SearchWho').on('change', function () {
        var array;
        $('#SearchBy').empty();
        $('#manager-table-first-header').html("");
        $('#first-table-data').html("");
        switch ($('#SearchWho').val()) {
            default:
                array = { StaffName: 'ФИО', StaffLogin: 'Логин' };
                break;
            case "GameGenre":
                array = { Game: 'Игра', Genre: 'Жанр' };
                break;
            case "GemesPrices":
                array = { Game: 'Игра', GameStartDate: 'Дата начала продаж', GameEndDate: 'Дата конца продаж', Price: 'Цена' };
                break;
            case "Genres":
                array = { GenreName: 'Жанр' };
                break;
            case "Inventory":
                array = { InventoryName: 'Инвентарь', GameName: 'Игра' };
                break;
            case "InventoryIPrices":
                array = { InventoryName: 'Инвентарь', InventoryStartDate: 'Дата начала продаж', InventoryEndDate: 'Дата конца продаж', Price: 'Цена' };
                break;
            case "Games":
                array = { GameName: 'Игра', GameCover: 'Обложка', DeveloperName: 'Разработчик', CountGameBay: 'Количество проданных копий', Prt: 'Приоритет' };
                break;
            case "Wallpapers":
                array = { WallpapersName: 'Обои', WallpapersPhoto: 'Изображение' };
                break;
            case "WallpaperPrice":
                array = { WallpapersName: 'Обои', WallpaperStartDate: 'Дата начала продаж', WallpaperEndDate: 'Дата конца продаж', Price: 'Цена' };
                break;
        }
        $.each(array, function (key, value) {
            if (key != 'GameCover' && key != 'WallpapersPhoto')$('#SearchBy').append('<option value="' + key + '">' + value + '</option>');
            $('#manager-table-first-header').append(`<th>${value}</th>`);
        });
    })
    $('#SearchBtn').on('click', function () {
        var SearchWho = $('#SearchWho').val();
        var SearchBy = $('#SearchBy').val();
        var SearchValue = $('#Search').val();
        var SetData = $('#first-table-data');
        SetData.html("");
        $.ajax({
            type: "post",
            url: "/Home/GetSearchingData?SearchWho="+SearchWho+"&SearchBy=" + SearchBy + "&SearchValue=" + SearchValue,
            contentType: "html",
            success: function (result) {
                if (result.length == 0) {
                    SetData.append("<tr style='color:#ff0000;'><td colspan='10'>Ничего не найдено</td></tr>");
                }
                else {
                    var SearchItem;
                    console.log(result);
                    $.each(result, function (index, value) {
                        switch (SearchWho){
                            default:
                                SearchItem = `<tr><td>${value.staffName}</td><td>${value.staffLogin}</td></tr>`;
                                break;
                            case "GameGenre":
                                SearchItem = `<tr><td>${value.gameName}</td><td>${value.genre}</td></tr>`;
                                break;
                            case "GemesPrices":
                                SearchItem = `<tr><td>${value.game}</td><td>${value.gameStartDate}</td><td>${value.gameEndDate}</td><td>${value.price}</td></tr>`;
                                break;
                            case "Genres":
                                SearchItem = `<tr><td>${value.genreName}</td></tr>`;
                                break;
                            case "Inventory":
                                SearchItem = `<tr><td>${value.inventoryName}</td><td>${value.gameName}</td></tr>`;
                                break;
                            case "InventoryIPrices":
                                SearchItem = `<tr><td>${value.inventoryName}</td><td>${value.gameStartDate}</td><td>${value.gameEndDate}</td><td>${value.price}</td></tr>`;
                                break;
                            case "Games":
                                SearchItem = `<tr><td>${value.gameName}</td><td style="min-width: 100px; min height: 50px; background-image: url(data:image/png;base64,${value.gameCover});bacground-repeat:no-repeat;background-size:cover;"></td><td>${value.developerName}</td><td>${value.countGameBay}</td><td>${value.prt}</td></tr>`;
                                break;
                            case "Wallpapers":
                                SearchItem = `<tr><td>${value.wallpapersName}</td><td style="min-width: 100px; min height: 50px; background-image: url(data:image/png;base64,${value.wallpapersPhoto});bacground-repeat:no-repeat;background-size:cover;"></td></tr>`;
                                break;
                            case "WallpaperPrice":
                                SearchItem = `<tr><td>${value.wallpapersName}</td><td>${value.wallpaperStartDate}</td><td>${value.wallpaperEndDate}</td><td>${value.price}</td></tr>`;
                                break;
                        }
                        SetData.append(SearchItem);
                    })
                }
            }
        })
    })
    $('#SearchEditWho').on('change', SearchEditWhoFunc);
    $('#SearchEditBtn').on('click', function () {
        var SearchWho = $('#SearchEditWho').val();
        var SearchBy = $('#SearchEditBy').val();
        var SearchValue = $('#SearchEdit').val();
        var SetData = $('#manageredit-first-table-data');
        var SetType = $('.manageredit-link-active').text();
        var objStr = "";
        if (SetType == "Изменить") $('.manageredit-form-table').css('display', 'none');
        if (SetType == "Добавить" || SetType == "Изменить") {
            objStr += `${$('#manageredit-second-table-data').children().eq(0).children().eq(0).val()}`;
            for (var i = 1; i < $('#manageredit-second-table-data').children().length; i++)
                objStr += `:${$('#manageredit-second-table-data').children().eq(i).children().eq(0).val()}`;
        }
        SetData.html("");
        if (SetType == "Добавить") {
            $.ajax({
                type: "post",
                url: "/Home/managerEditFunc?type=" + typeUpdate + "&SearchWho=" + SearchWho + "&SearchBy=" + SearchBy + "&SearchValue=" + SearchValue + "&EditValue=" + objStr,
                contentType: "html",
                success: function (result) {
                    if (result == "true") {
                        $('#eventStatus').css("color", "#1F3671");
                        $('#eventStatus').text("Статус действия: выполнено");
                    }
                    else {
                        $('#eventStatus').css("color", "#ff0000");
                        $('#eventStatus').text(result);
                    }
                }
            })
        }
        else {
            $.ajax({
                type: "post",
                url: "/Home/managerEditUpdate?idtable=" + idtable + "&EditValue=" + objStr,
                contentType: "html",
                success: function (result) {
                    if (result == "true") {
                        $('#eventStatus').css("color", "#1F3671");
                        $('#eventStatus').text("Статус действия: выполнено");
                    }
                    else {
                        $('#eventStatus').css("color", "#ff0000");
                        $('#eventStatus').text(result);
                    }
                }
            })
            setTimeout(() => { searchEvent() },200)
        }
    })
    $('.manageredit-main-input').on('change', function (e) {
        e.target.value = (e.target.value.replace(/[:!?&\*\+\(\)\=\_\@\#\$\%\^\`\~\<\>\;]/g, ''));
    })
    $('#search-form-btn').on('click', changeStore);
    $('input[name=order]').on('change', changeStore);
    $('input[name=Type]').on('change', changeTypeStore);
    $('input[name=Genre]').on('change', changeStore);
    $('#SearchEditSearchBtn').on('click', searchEvent);
    $('input[name=submitEvent-me]').on('click',updOrDelEvent)
})
function changeTypeStore(e) {
    console.log(e.target.id);
    switch (e.target.id) {
        default:
            $('.form-genre').fadeTo('fast', 0);
            break;
        case "gameRadio":
            $('.form-genre').fadeTo('fast', 1);
            break;
    }
    changeStore();
}
function changeStore() {
    var login = getCookie('ClientLogin');
    var Gwrapper = $('.games-wrapper');
    var GIWval = $('input[name=Type]:checked').val();
    var GIWname = $('#search-form-search').val();
    var GIWorder = $('input[name=order]:checked').val();
    var GIWgenresMass = $('input[name=Genre]:checked');
    var GIWgenres = "";
    switch (GIWorder) {
        default:
            GIWorder = "Name";
            break;
        case "По убывающей цене":
            GIWorder = "PriceMinus";
            break;
        case "По возрастающей цене":
            GIWorder = "PricePlus";
            break;
    }
    if (GIWgenresMass.length!=0) {
        GIWgenres = GIWgenresMass.eq(0).val();
        for (var i = 1; i < GIWgenresMass.length; i++)
            GIWgenres += `:${GIWgenresMass.eq(i).val()}`;
    }
    Gwrapper.html("");
    Gwrapper.fadeTo(0,0);
    $.ajax({
        type: "post",
        url: "/Home/GIWSearchJson?GIWval=" + GIWval + "&GIWname=" + GIWname + "&GIWgenres=" + GIWgenres + "&GIWorder=" + GIWorder,
        contentType: "html",
        success: function (result) {
            switch (result.giw) {
                default:
                    for (var i = 0; i < result.gl.length; i++)
                        Gwrapper.append(`<div class="items">` +
                            `<div class="img" style="background-image: url(data:image/png;base64,${result.gl[i].gamecover});"></div>` +
                            `<div class="text">` +
                            `<p class="StoreName">${result.gl[i].gamename}</p>` +
                            `<p class="StorePrice">Цена: ${result.gl[i].price}</p>` +
                            `<p class="StoreGenre">Жанры: ${result.gl[i].genre}</p>` +
                            `<p class="StoreGenre">Разработчик: ${result.gl[i].dev}</p></div>` +
                            `<div class="btn"><a href="/Home/BuyGame?GameName=${result.gl[i].gamename}">Купить</a></div></div>`);
                    break;
                case "I":
                    for (var i = 0; i < result.il.length; i++)
                        if (login != null)
                            Gwrapper.append(`<div class="items"><div class="inv-text">` +
                                `<p class="StorePrice">${result.il[i].inventoryName}</p>` +
                                `<p class="StoreGenre">Цена: ${result.il[i].price}</p>`+
                                `<p class="StoreGenre">Игра: ${result.il[i].gameName}</p></div>` +
                                `<div class="inv-btn"><a href="/Home/BuyGIW?GIW=I&amp;ItemName=${result.il[i].inventoryName}">Купить</a></div></div>`);
                        else Gwrapper.append(`<div class="items"><div class="inv-text">` +
                                `<p class="StorePrice">${result.il[i].inventoryName}</p>` +
                                `<p class="StoreGenre">Цена: ${result.il[i].price}</p>` +
                                `<p class="StoreGenre">Игра: ${result.il[i].gameName}</p></div>` +
                                `<div class="inv-btn"><a href="/Home/Registration">Зарегистрироваться</a></div></div>`);
                    break;
                case "W":
                    for (var i = 0; i < result.wl.length; i++)
                        if (login != null)
                            Gwrapper.append(`<div class="items">` +
                                `<div class="img" style="background-image: url(data:image/png;base64,${result.wl[i].wallPhoto});"></div>` +
                                `<div class="text"><p class="StoreName">${result.wl[i].wallName}</p>` +
                                `<p class="StorePrice">Цена: ${result.wl[i].price}</p></div>` +
                                `<div class="btn"><a href="/Home/BuyGIW?GIW=W&amp;ItemName=${result.wl[i].wallName}">Купить</a></div></div>`);
                        else Gwrapper.append(`<div class="items">` +
                                `<div class="img" style="background-image: url(data:image/png;base64,${result.wl[i].wallPhoto});"></div>` +
                                `<div class="text"><p class="StoreName">${result.wl[i].wallName}</p>` +
                                `<p class="StorePrice">Цена: ${result.wl[i].price}</p></div>` +
                                `<div class="inv-btn"><a href="/Home/Registration">Зарегистрироваться</a></div></div>`);
                    break;

            }
            Gwrapper.fadeTo('fast', 1);
            $('#search-form-search').val('');
        }
    })
}
function chManagerEditBtn(e) {
    e.preventDefault;
    $('#SearchEditWho option:first').prop('selected', true);
    $('.manageredit-link-active').removeClass('manageredit-link-active');
    e.classList.add('manageredit-link-active');
    SearchEditWhoFunc();
    switch (e.textContent) {
        case "Добавить":
            typeUpdate = "Add";
            $('.manageredit-form-table').css('display', 'flex');
            $('#SearchEdit').fadeTo("fast", 0);
            $('#SearchEditBy').fadeTo("fast", 0);
            $('.table-search-inf').css('display', 'none');
            $('#SearchEditSearchBtn').fadeTo("fast", 0);
            break;
        case "Удалить":
            typeUpdate = "Remove";
            $('.manageredit-form-table').css('display', 'none');
            $('.table-search-inf').css('display', 'flex');
            $('#SearchEditSearchBtn').fadeTo("fast",1);
            $('#SearchEdit').fadeTo("fast", 1);
            $('#SearchEditBy').fadeTo("fast", 1);
            $('#SearchEditSearchBtn').fadeTo("fast", 1);
            break;
        case "Изменить":
            typeUpdate = "Update";
            $('.manageredit-form-table').css('display', 'none');
            $('.table-search-inf').css('display', 'flex');
            $('#SearchEdit').fadeTo("fast", 1);
            $('#SearchEditBy').fadeTo("fast", 1);
            $('#SearchEditSearchBtn').fadeTo("fast", 1);
            break;
    }
}
function validateLogin(loginString) {
    pattern = /^[a-zA-Z](.[a-zA-Z0-9_-]*)$/;
    return pattern.test(loginString);
}
function getCookie(name) {
    var cookie = " " + document.cookie;
    var search = " " + name + "=";
    var setStr = null;
    var offset = 0;
    var end = 0;
    if (cookie.length > 0) {
        offset = cookie.indexOf(search);
        if (offset != -1) {
            offset += search.length;
            end = cookie.indexOf(";", offset)
            if (end == -1) {
                end = cookie.length;
            }
            setStr = unescape(cookie.substring(offset, end));
        }
    }
    return (setStr);
}
function SearchEditWhoFunc() {
    var array;
    $('#SearchEditBy').empty();
    $('#manageredit-table-first-header').html("");
    $('#manageredit-table-first-header-search').html("");
    $('#manageredit-first-table-data').html("");
    $('#manageredit-second-table-data').html("");
    $('#manager-edit-table-search-date').html("");
    switch ($('#SearchEditWho').val()) {
        default:
            array = { StaffName: 'ФИО', StaffLogin: 'Логин' };
            break;
        case "GameGenre":
            array = { Game: 'Игра', Genre: 'Жанр' };
            break;
        case "GemesPrices":
            array = { Game: 'Игра', GameStartDate: 'Дата начала продаж', GameEndDate: 'Дата конца продаж', Price: 'Цена' };
            break;
        case "Genres":
            array = { GenreName: 'Жанр' };
            break;
        case "Inventory":
            array = { InventoryName: 'Инвентарь', GameName: 'Игра' };
            break;
        case "InventoryIPrices":
            array = { InventoryName: 'Инвентарь', InventoryStartDate: 'Дата начала продаж', InventoryEndDate: 'Дата конца продаж', Price: 'Цена' };
            break;
        case "WallpaperPrice":
            array = { WallpapersName: 'Обои', WallpaperStartDate: 'Дата начала продаж', WallpaperEndDate: 'Дата конца продаж', Price: 'Цена' };
            break;
    }
    $('#manageredit-table-first-header-search').append(`<th>Действие</th>`);
    $.each(array, function (key, value) {
        $('#SearchEditBy').append('<option value="' + key + '">' + value + '</option>');
        if (key.indexOf('Date') != -1) $('#manageredit-second-table-data').append(`<td><input id="${key}" type="date" name="editInputMng" value="" /></td>`);
        else if (key == "InventoryName" && array["GameName"] == "Игра") $('#manageredit-second-table-data').append(`<td><input id="${key}" type="text" name="editInputMng" value="" /></td>`);
        else getTypeBox(key);
        $('#manageredit-table-first-header').append(`<th>${value}</th>`);
        $('#manageredit-table-first-header-search').append(`<th>${value}</th>`);
    });
}
function getTypeBox(key) {

    if (key == "Price" || key == "StaffName" ||
        key == "GenreName" || key == "StaffName" ||
        key == "StaffLogin") {
        $('#manageredit-second-table-data').append(`<td><input id="${key}" type="text" name="editInputMng" value="" /></td>`);
        return;
    }
    $('#manageredit-second-table-data').append(`<td><select id="${key}"></select></td>`);
    $.ajax({
        type: "post",
        url: "/Home/getSelectOptions?key=" + key,
        contentType: "html",
        success: function (result) {
            $.each(result, function (index, value) {
                $(`#${key}`).append(`<option value="${value}">${value}</option>`);
            })
        }
    })
}
function searchEvent() {
    var SearchBy = $('#SearchEditWho').val();
    var SearchValue = $('#SearchEdit').val();
    var SearchEditBy = $('#SearchEditBy').val();
    var tableRes = $('#manager-edit-table-search-date');
    var SearchItem = "";
    tableRes.html("");
    $.ajax({
        type: "post",
        url: "/Home/getSearchEdit?SearchBy=" + SearchBy + "&SearchEditBy=" + SearchEditBy + "&SearchValue=" + SearchValue,
        contentType: "html",
        success: function (result) {
            $.each(result, function (index, value) {
                switch (SearchBy) {
                    default:
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.staffID}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /></td><td>${value.staffName}</td><td>${value.staffLogin}</td></tr>`;
                        break;
                    case "GameGenre":
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.ggid}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /><td>${value.gameName}</td><td>${value.genre}</td></tr>`;
                        break;
                    case "GemesPrices":
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.buyGameID}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /><td>${value.game}</td><td>${value.gameStartDate}</td><td>${value.gameEndDate}</td><td>${value.price}</td></tr>`;
                        break;
                    case "Genres":
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.genreID}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /><td>${value.genreName}</td></tr>`;
                        break;
                    case "Inventory":
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.inventoryID}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /><td>${value.inventoryName}</td><td>${value.gameName}</td></tr>`;
                        break;
                    case "InventoryIPrices":
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.bayInventoryID}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /><td>${value.inventoryName}</td><td>${value.inventoryStartDate}</td><td>${value.inventoryEndDate}</td><td>${value.price}</td></tr>`;
                        break;
                    case "WallpaperPrice":
                        SearchItem = `<tr><td><input onclick="updOrDelEvent(this)" id="${value.buyWallpaperID}:${SearchBy}" type="submit" name="submitEvent-me" value="+" /><td>${value.wallpapersName}</td><td>${value.wallpaperStartDate}</td><td>${value.wallpaperEndDate}</td><td>${value.price}</td></tr>`;
                        break;
                }
                tableRes.append(SearchItem);
            })
        }
    })
}
function updOrDelEvent(e) {
    idtable = e.id;
    switch (typeUpdate) {
        default:
            $('.manageredit-form-table').css('display', 'flex');
            var mass = $('input[name=editInputMng]');
            for (var i = 0; i < mass.length; i++)
                mass.eq(i).val("");
            return;
        case "Remove":
            $.ajax({
                type: "post",
                url: "/Home/eventRemoveJson?idtable=" + idtable,
                contentType: "html",
                success: function (result) {
                    if (result == "true") {
                        $('#eventStatus').css("color", "#1F3671");
                        $('#eventStatus').text("Статус действия: выполнено");
                    }
                    else {
                        $('#eventStatus').css("color", "#ff0000");
                        $('#eventStatus').text(result);
                    }
                }
            })
            setTimeout(() => { searchEvent(); }, 200);
            return;
    }
}
var idtable = "Add";
var typeUpdate = "Add";
var strEditManager;
var validEdit = true;