import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

@Component({
  selector: 'app-pagination',
  imports: [],
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaginationComponent {
  pageNumber = input.required<number>();
  pageSize = input.required<number>();
  totalCount = input.required<number>();
  pageSizeOptions = input<number[]>([5, 10, 25, 50]);

  pageChange = output<number>();
  pageSizeChange = output<number>();

  totalPages = computed(() => {
      return Math.ceil(this.totalCount() / this.pageSize());
  });

  startItem = computed(() => 
    (this.pageNumber() - 1) * this.pageSize() + 1
  );

  endItem = computed(() => 
    Math.min(this.pageNumber() * this.pageSize(), this.totalCount())
  );

  pageNumbers = computed<(number | string)[]>(() => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;
    const totalPage = this.totalPages();
    const pageNumber = this.pageNumber();

    if(totalPage <= maxVisible + 2) {
        for(let i = 1; i <= totalPage; i++) {
            pages.push(i);
        }
    } else {
        if(pageNumber <= 3) {
            for(let i = 1; i <= maxVisible; i++) {
                pages.push(i);
            }
            pages.push('...');
            pages.push(totalPage);
        } else if(pageNumber >= totalPage - 2) {
            pages.push(1);
            pages.push('...');
            for(let i = totalPage - maxVisible + 1; i <= totalPage; i++) {
                pages.push(i);
            }
        } else {
            pages.push(1);
            pages.push('...');
            for(let i = pageNumber - 1; i <= pageNumber + 1; i++) {
                pages.push(i);
            }
            pages.push('...');
            pages.push(totalPage);
        }
    }

    return pages;
  });

  isNumber(v: number | string): v is number {
      return typeof v === 'number';
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages() && page !== this.pageNumber()) {
      this.pageChange.emit(page);
    }
  }

  onPageSizeChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    const newSize = parseInt(target.value, 10);
    this.pageSizeChange.emit(newSize);
  }
}
