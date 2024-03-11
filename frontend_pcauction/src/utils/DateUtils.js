export const formatDate = (date) => {
    const dateTime = new Date(date);
    const offsetMinutes = dateTime.getTimezoneOffset();
    const utcTime = dateTime.getTime() + offsetMinutes * 60 * 1000;
    const utcDateTime = new Date(utcTime);
    const formattedDateTime = utcDateTime.toLocaleString();
    return formattedDateTime;
  }
