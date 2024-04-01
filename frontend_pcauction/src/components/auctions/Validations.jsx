export const isValidTitle = (title) => {
    return title && title.length >= 5 && title.length <= 45;
}

export const isValidDescription = (description) => {
    return description && description.length >= 10;
}

export const isDatePastNow = (date) =>
{
    const currentDate = new Date();
    const newDate = new Date(date);
    return newDate > currentDate;
}

export const isEndDateLater = (startDate, endDate) =>
{
    const startDateTime = new Date(startDate);
    const endDateTime = new Date(endDate);
    return endDateTime > startDateTime;
}

export const isValidMinInc = (minInc) => {
    return minInc !== null && minInc !== "" && minInc >= 0;
}
