export const capitalize = (text: string) => {
  if (text.length == 0) throw new Error("text is empty");
  return text.charAt(0).toUpperCase() + text.slice(1);
};
