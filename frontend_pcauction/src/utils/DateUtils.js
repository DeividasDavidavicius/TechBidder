export const formatDate = (date) => {
    const dateTime = new Date(date);
    const offsetMinutes = dateTime.getTimezoneOffset();
    const utcTime = dateTime.getTime() + offsetMinutes * 60 * 1000;
    const utcDateTime = new Date(utcTime);
    const formattedDateTime = utcDateTime.toLocaleString();
    return formattedDateTime;
  }


  export const timeLeft = (date1Str, date2Str) =>
  {
    const date1 = new Date(date1Str);
    const date2 = new Date(date2Str);

    const differenceMs = Math.abs(date1 - date2);

    const days = Math.floor(differenceMs / (1000 * 60 * 60 * 24));
    const hours = Math.floor((differenceMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((differenceMs % (1000 * 60 * 60)) / (1000 * 60));

    let result = '';
    if (days > 0) {
      result += `${days} days, `;
    }
    if (hours > 0) {
      result += `${hours} hours, `;
    }
    result += `${minutes} minutes`;
    return result;
  }
