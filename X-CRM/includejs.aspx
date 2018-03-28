<script>


    $(document).ready(function (e) {
        /*$(".databind").select2({
            width : '400'
        });*/


    });



    ////

    function _savePreLine(url, screen, frm, dv) {
        var re = "";
        var rows = "";
        $.each($("#" + dv + ">table>tbody>tr:last"), function (i, v) {
            $.each($(this).find("td"), function (ia, va) {

                //alert($(va).find(".frm").attr("name"));
                $.each($(va).find(".frm"), function (ib, vb) {
                    if ($(vb).attr("name") != null && $(vb).val() != null) {
                        
                        rows = rows + "&" + $(vb).attr("name") + "=" +
                            encodeURIComponent(($(vb).val()));
                        $(vb).val("");
                    }
                });

            });
        });

        //alert(rows);
        $.ajax({
            url: url,
            data: "app=getPreSaveLine&screen=" + screen + "&rowNum=" +
                ($("#" + dv + ">table>tbody>tr").length - 1) + "&" +
                rows,
            type: "POST",
            async: false,
            error: function () {
                return "error";
            },
            beforeSend: function () {
                metroDialog.open('#dvLoadingOut');
            },
            success: function (data) {
                metroDialog.close('#dvLoadingOut');
                re = data;
            }
        });
        return re;
    }

    function _saveRecord(frm, screen, url) {
        var re = "";
        $(".notify-container").html("");
        $.ajax({
            url: url,//"<%Response.Write(Session["incurl"] == null ? Session["url"] + "include.aspx" : Session["incurl"]);%>",
            data: "app=saveRecord&screen=" + screen + "&" + $("." + frm).serialize(),
            type: "POST",
            async: false,
            error: function () {
                return "error";
            },
            beforeSend: function () {
                metroDialog.open('#dvLoadingOut');

            },
            success: function (data) {
                re = setError(data, frm);
                metroDialog.close('#dvLoadingOut');
                /*
                var onceError = "";
                var errStr = "";
                data = $.parseJSON(data);
                $.each(data, function (i, v) {
                    re = i;
                    if (i == "error") {
                        $.each($("." + frm).find("div.error"), function (d, v) {
                            $(v).removeClass("error");
                        });

                        data = data["error"];
                        $.each(data, function (d, v) {
                            if (onceError == "" && v.errType == "once") {
                                onceError = "<li>All Fields mark with (*) are required</li>";
                            }
                            if (v.errType != "once") {

                                errStr = errStr + "<li>" + v.msg + "</li>";
                            }
                            $("." + frm).find("#" + v.colName).closest("div").addClass("error");
                        });

                        $.Notify({
                            caption: '<strong>Error Saving Record !</strong>',
                            content: "<ul>" + onceError + errStr + "</ul>",
                            type: 'alert',
                            keepOpen: true
                            //,icon: "<span class='mif-vpn-publ'></span>"
                        });
                        return "error";
                    } else {
                        data = data["tbl"];
                        re = data;
                    }
                });
                */
            }
        });
        return re;
    }

    function setError(data, frm) {
        $(".notify-container").html("");
        var re = "";
        var onceError = "";
        var errStr = "";
        data = $.parseJSON(data);
        $.each(data, function (i, v) {
            re = i;
            if (i == "error") {
                $.each($("." + frm).find("div.error"), function (d, v) {
                    $(v).removeClass("error");
                });

                data = data["error"];
                $.each(data, function (d, v) {
                    if (onceError == "" && v.errType == "once") {
                        onceError = "<li>All Fields mark with (*) are required</li>";
                    }
                    if (v.errType != "once") {

                        errStr = errStr + "<li>" + v.msg + "</li>";
                    }
                    $("." + frm).find("#" + v.colName).closest("div").addClass("error");
                });

                $.Notify({
                    caption: '<strong>Error Saving Record !</strong>',
                    content: "<ul>" + onceError + errStr + "</ul>",
                    type: 'alert',
                    keepOpen: true
                    //,icon: "<span class='mif-vpn-publ'></span>"
                });
                return "error";
            } else {
                data = data["tbl"];

                re = data;

            }
        });
        return re;
    }

    function _delRecord(frm, screen, eid, url, dv) {
        re = "";
        if (dv == "")
            dv = "dvContent";
        $.ajax({
            url: url,
            data: "app=delRecord&screen=" + screen + "&eid=" + eid,
            type: "POST",
            async: false,
            error: function () {
                return "error";
            },
            beforeSend: function () {
                $("#" + dv).html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
            },
            success: function (data) {
                if (data == "ok") {
                    re = "ok";
                } else {
                    re = (data);
                }
            }
        });
        return re;
    }



    function _findRecord(frm, screen, url, cPage, dv) {
        if (dv == "") {
            dv = "dvList";
        }
        var sort = "";
        if ($("#" + dv).find("#orderBy").length > 0) {
            sort = sort + "&orderBy=" + $("#" + dv).find("#orderBy").val();
        }
        if ($("#" + dv).find("#orderFieldBy").length > 0) {
            sort = sort + "&orderFieldBy=" + $("#" + dv).find("#orderFieldBy").val();
        }

        $.ajax({
            url: url,
            data: "app=findRecord&screen=" + screen + "&cPage=" + cPage + "&" + $("." + frm).serialize() + sort,
            type: "POST",
            async: false,
            error: function () {
                return "error";
            },
            beforeSend: function () {
                $("#" + dv).html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
            },
            success: function (data) {
                $("#" + dv).html(data);
                $(".select").select2();
            }
        });
    }

    function _loadScreen(screen, mode, eid, dv, url) { // 3 edit // 2 View
        $.ajax({
            url: url,
            data: "app=loadScreen&screen=" + screen + "&mode=" + mode + "&eid=" + eid,
            type: "POST",
            async: false,
            error: function () {
                return "error";
            },
            beforeSend: function () {
                $("#" + dv).html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
            },
            success: function (data) {
                $("#" + dv).html(data);
                initComp(url);
            }
        });
    }

    //

    function _delLine(pos, v) {
        if (confirm("Delete Current Record ?")) {
            $(v).closest("tr").find("input[name='txtDel" + pos + "']").val("1");
            $(v).closest("tr").hide();
        }
    }

    function initComp(url) {
        $(".select").select2();
        $.ajax({
            url: "<%=url%>include.aspx",
            data: "app=setSSA",
            type: "POST",
            async: false,
            error: function () {
                return "error";
            },
            beforeSend: function () {

            },
            success: function (data) {

                try {
                    data = $.parseJSON(data);
                    data = data["tbl"];
                    $.each(data, function (i, v) {

                        setSSA(v, url);
                    });
                    //$(".select2-dropdown").width(400);
                } catch (e) {

                }
            }
        });

    }

    function setSSA(ids, url) {

        if (url + "" == "undefined" || url == "") {
            url = "<%=url%>include.aspx";
        }

        //ids = $.parseJSON(ids);
        $.each(ids, function (i, id) {
            
            $.each($("." + id.colName), function (i, v) {
                //    if($(v).hasClass("init_WarehouseID"))
                //    alert($(v).attr("id"));
                try {
                    /*
                    var cols = id["cols"];
                    sf = "'text','id',";
                    $.each(cols, function (ia, ida) {

                        sf = sf + "'" + ida.DisCol + "',";
                    });
                    sf = "[" + sf.substr(0,sf.length-1) + "]";
                    $(v).selectize({
                        width: 'resolve',
                        valueField: 'id',
                        labelField: 'text',
                        searchField: eval(sf),
                        render: {
                            option: function (item, escape) {
                                var col = "";
                                $.each(cols, function (ia, ida) {
                                    col = col + '<div class="cell" style="width:100%">' + getJsonStr(item[ida.DisCol]) + '</div>';
                                });
                                
                                
                                return(
                                    '<div class="grid">' +
                                        '<div class="row cells' + cols.length + '">' +
                                            col +
                                        '</div>' +
                                        "<div class='ClearFix'></div><hr class='thin bg-grayLighter'/>" +
                                    '</div>'
                                );
                            }
                        },
                        load: function (query, callback) {
                            if (!query.length) return callback();
                            $.ajax({
                                url: url ,//+ '?app=SSA&colid=' + id.colid + "&colName=" + id.colName + "&" + $("form").serialize(),
                                type: 'POST',
                                dataType: 'json',
                                //data: {
                                //    q: query,
                                //    page_limit: 10,
                                //    apikey: '3qqmdwbuswut94jv4eua3j85'
                                //},
                                data: 'q=' + query + '&page_limit=10' + '&app=SSA&colid=' + id.colid + "&colName=" + id.colName + "&" + $("form").serialize(),
                                error: function () {
                                    callback();
                                },
                                success: function (res) {
                                   
                                    callback(res);
                                }
                            });
                        }
                    });
                    */
                    $(v).select2({
                        placeholder: "Select one",
                        allowClear: true,
                        templateResult: function (data) {

                            var col = "";
                            var cols = id["cols"];

                            $.each(cols, function (ia, ida) {
                                col = col + '<div class="cell" style="width:100%">' + getJsonStr(data[ida.DisCol]) + '</div>';
                            });

                            var $result = $(
                                '<div class="grid">' +
                                    '<div class="row cells' + cols.length + '">' +
                                        col +
                                    '</div>' +
                                    "<div class='ClearFix'></div><hr class='thin bg-grayLighter'/>" +
                                '</div>'
                            );

                            return $result;
                        },
                        ajax: {
                            type: 'POST',
                            data: function (params)
                            {
                                return 'app=SSA&colid=' + id.colid + "&colName=" + id.colName + "&" + $("form").serialize() + '&q=' + params.term
                            },//'app=SSA&colid=' + id.colid + "&colName=" + id.colName + "&" + $("form").serialize(),
                            url: function () {
                                return url// + '?app=SSA&colid=' + id.colid + "&colName=" + id.colName + "&" + $("form").serialize()
                            },// url + '?app=SSA&colid=' + id.colid + "&colName=" + id.colName + "&" + $("form").serialize(),
                            dataType: 'json',
                            delay: 250,
                            processResults: function (data) {
                                return {

                                    results: data
                                };

                            },
                            cache: true
                        },
                        minimumInputLength: 1
                    });
                    
                } catch (e) {
                    //alert(e.message);
                }
                

            });

        });
        /*
        $('select2-search-field > input.select2-input').on('keyup', function (e) {
            if (e.keyCode === 13)
                alert('enter key event');
        });*/
    }

    // util

    function getJsonStr(str) {
        if (str != null) {
            return str;
        } else {
            return "";
        }
    }

    function getJsonStr2(str, str2) {
        if (str != null) {
            return str + str2;
        } else {
            return "";
        }
    }

    // Read a page's GET URL variables and return them as an associative array.
    function getUrlVars(url) {
        var vars = [], hash;
        var hashes = url.slice(url.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    }
</script>
