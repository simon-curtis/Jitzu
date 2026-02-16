/** Shared color palette for all syntax highlighting and terminal rendering. */
export const theme = {
  // Shell prompt colors
  user: "#5f8787",
  dir: "#87d7ff",
  arrow: "#5faf5f",
  branch: "#808080",
  dirty: "#d7af87",

  // ls output colors
  lsDirectory: "#87afd7",
  lsExecutable: "#87af87",
  lsArchive: "#d75f5f",
  lsMedia: "#af87af",
  lsCode: "#87afaf",
  lsConfig: "#d7af87",
  lsSize: "#87af87",

  // Error
  error: "#d75f5f",

  // Language token colors
  keyword: "#87afd7",
  string: "#afaf87",
  number: "#d7af87",
  boolean: "#d7af87",
  function: "#87af87",
  type: "#af87af",
  comment: "#808080",
  operator: "#af87af",
  property: "#87afaf",
  command: "#87af87",
  flag: "#87afaf",
  dim: "#808080",
  text: "#c8c8c8",
} as const;
