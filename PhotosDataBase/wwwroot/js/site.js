﻿"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/photossearch").build();
var cancelConnection = new signalR.HubConnectionBuilder().withUrl("/photossearch").build();

connection.on("ShowServerError", function (message) {
    console.error("Server error: " + message);
    document.getElementById("errMessage").innerText = message;

    document.getElementById("dirNameInput").removeAttribute("disabled");
    document.getElementById("searchFiles").removeAttribute("disabled");
    document.getElementById("cancelLoading").setAttribute("disabled", "disabled");
});

connection.on("ShowLoadingProcess", function (allCount, loadedCount) {
    var percent = (loadedCount / allCount * 100).toFixed(2);
    document.getElementById("loadedFilesCount").innerText = "Loading photos: " + loadedCount + " / " + allCount;
    document.getElementById("progressBar").innerText = percent + "%";
    document.getElementById("progressBar").style.width = percent + "%";
});

connection.on("ShowImportResult", function () {
    document.getElementById("loadedFilesCount").innerText = "All photos are loaded";
    document.getElementById("progressDiv").style.display = "none";
    document.getElementById("dirNameInput").removeAttribute("disabled");
    document.getElementById("searchFiles").removeAttribute("disabled");
    document.getElementById("cancelLoading").setAttribute("disabled", "disabled");
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
cancelConnection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("searchFiles").addEventListener("click", function (event) {
    var basePath = document.getElementById("dirNameInput").value;
    
    connection.invoke("LoadPhotos", basePath).catch(function (err) {
        document.getElementById("errMessage").innerText = err.toString();
        return console.error(err.toString());
    });

    document.getElementById("progressBar").innerText = "0%";
    document.getElementById("progressBar").style.width = "0%";
    document.getElementById("progressDiv").style.display = "block";
    document.getElementById("dirNameInput").setAttribute("disabled", "disabled");
    document.getElementById("searchFiles").setAttribute("disabled", "disabled");
    document.getElementById("cancelLoading").removeAttribute("disabled");

    event.preventDefault();
});

document.getElementById("cancelLoading").addEventListener("click", function (event) {

    cancelConnection.invoke("CancelLoading").catch(function (err) {
        document.getElementById("errMessage").innerText = err.toString();
        return console.error(err.toString());
    });

    document.getElementById("dirNameInput").removeAttribute("disabled");
    document.getElementById("searchFiles").removeAttribute("disabled");
    document.getElementById("cancelLoading").setAttribute("disabled", "disabled");

    event.preventDefault();
});
