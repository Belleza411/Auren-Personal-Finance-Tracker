export interface SlidePanel {
    startClose: () => void;
}

export interface SlidePanelConfig<TData> {
  data?: TData;
  width?: string;
  height?: string;
  position?: {
    top?: string;
    bottom?: string;
    left?: string;
    right?: string;
  }
}