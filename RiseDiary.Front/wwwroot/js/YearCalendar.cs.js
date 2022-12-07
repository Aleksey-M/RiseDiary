"use strict"

export function initCalendar(elementCssSelector, dates, onClickEventHandler, dotnetCallbacks) {
    try {

        const displayDates = dates.map((d) => ({
            startDate: new Date(d),
            endDate: new Date(d),
            name: d,
            id: d
        }));

        const year = new Date().getFullYear();

        const calendar = new Calendar(elementCssSelector, {
            style: 'background',
            enableRangeSelection: true,
            displayHeader: false,
            selectRange: function (e) {
                const y = e.startDate.getFullYear();
                const m = e.startDate.getMonth() + 1;
                const d = e.startDate.getDate();

                const z = n => ('0' + n).slice(-2);

                onClickEventHandler.invokeMethodAsync('OnClick', `${y}-${z(m)}-${z(d)}`);
            },
            startYear: year,
            maxDate: new Date(year, 11, 31),
            minDate: new Date(year, 0, 1),
            dataSource: displayDates
        });

        dotnetCallbacks.invokeMethodAsync('Completed');
    }
    catch (e) {
        console.log(e.message);
        dotnetCallbacks.invokeMethodAsync('Error', e.message);
    }    
}
