export const futureDate = ({ value }: { value: () => string | Date }) => {
    if(!value()) return null;

    const inputDate = new Date(value());

    if(isNaN(inputDate.getTime())) return null;

    const today = new Date();

    today.setHours(0, 0, 0, 0);

    if (inputDate <= today) {
      return {
        kind: 'invalidInput',
        message: 'Target date must be after today'
      };
    }
    return null;
  };