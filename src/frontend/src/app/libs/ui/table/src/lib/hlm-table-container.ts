import { Directive } from "@angular/core";
import { classes } from "@spartan-ng/helm/utils";

@Directive({
	selector: 'div[hlmTableContainer]',
	host: { 'data-slot': 'table-container' },
})
export class HlmTableContainer {
	constructor() {
		classes(() => 'relative w-full overflow-x-auto');
	}
}