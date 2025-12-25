import { Category } from "../../features/categories/models/categories.model";

export function findCategoryName(categories: Category[], id: string)  {
    return categories.find(c => c.categoryId === id)?.name ?? '';
}