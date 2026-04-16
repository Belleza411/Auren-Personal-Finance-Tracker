export interface SlidePanel {
    startClose: () => void;
}

export interface SlidePanelConfig<TData> {
  data?: TData;
  width?: string;
}