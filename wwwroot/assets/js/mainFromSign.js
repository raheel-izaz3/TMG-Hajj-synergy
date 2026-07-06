// Show Date 

function showDate(date) {
    let d = new Date(date)
    document.getElementById('date').innerText = `${d.getDate()}/${d.getMonth() + 1}/${d.getFullYear()}`;
}



const currentYearGregorian = new Date().getFullYear();
// document.getElementById('yearGregorian').innerText = currentYearGregorian

function gregorianToHijri(date) {
    const gregorianDate = new Date(date);
    const utcDate = Date.UTC(
        gregorianDate.getUTCFullYear(),
        gregorianDate.getUTCMonth(),
        gregorianDate.getUTCDate()
    );
    const msPerDay = 24 * 60 * 60 * 1000;
    const hijriDate = new Date(
        Math.floor((utcDate - msPerDay + 1.5 * msPerDay) / msPerDay) +
        1948439
    );
    const hijriYear = hijriDate.getUTCFullYear();
    return hijriYear;
}


const currentYearHijri = gregorianToHijri(new Date());

