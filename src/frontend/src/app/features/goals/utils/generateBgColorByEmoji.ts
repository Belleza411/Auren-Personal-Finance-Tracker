import chroma from 'chroma-js';

export const generateBgColorByEmoji = (emoji: string): string => {
  if (!emoji) return '#ccc';

  const code = [...emoji].reduce((sum, char) => 
    sum + char.codePointAt(0)!, 0);

  const base = chroma.hsl(code % 360, 0.6, 0.55);

  return base.luminance() < 0.35
    ? base.brighten(1).hex()
    : base.darken(0.5).hex();
}

