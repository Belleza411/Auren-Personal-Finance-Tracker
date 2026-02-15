export const greaterThanZero = (fieldName: string) => ({ value }: { value: () => number }) => {
    const v = value();
    if(v == null) return null;

    if (v <= 0) {
        return {
        kind: 'invalidInput',
        message: `${fieldName} must be greater than 0`
        };
    }
    return null;
};