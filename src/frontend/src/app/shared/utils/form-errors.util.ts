import { computed, Signal } from "@angular/core";
import { FieldState } from "@angular/forms/signals";

type FormFields<T> = {
    [K in keyof T]: () => FieldState<T[K]>;
}

type FieldErrors<T> = {
    [K in keyof T]: Signal<boolean>;
}

export function createFieldErrors<T extends object>(
    fields: FormFields<T>
): FieldErrors<T> {
    return Object.fromEntries(
        Object.entries(fields).map(([key, getField]) => [
            key,
            computed(() => {
                const field = (getField as () => FieldState<unknown>)();
                return field.invalid() && field.touched();
            })
        ]
    )) as FieldErrors<T>;
}