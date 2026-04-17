/**
 * Transforms raw MDX source into plain markdown suitable for LLM consumption.
 * Strips imports/exports, converts <CodeBlock> JSX to fenced code, drops other JSX.
 */
export function mdxToMarkdown(body: string): string {
  let md = body;

  // Strip import/export lines (MDX-only syntax)
  md = md.replace(/^\s*(?:import|export)\s+[^\n]+;?\s*$/gm, '');

  // <CodeBlock language="X" code={`Y`} />  →  ```X\nY\n```
  md = md.replace(
    /<CodeBlock\s+language="([^"]+)"\s+code=\{`([\s\S]*?)`\}\s*\/>/g,
    (_, lang, code) => `\`\`\`${lang}\n${code}\n\`\`\``,
  );

  // <DownloadButtons /> → link to install page
  md = md.replace(/<DownloadButtons\s*\/>/g, '_See [Installation](/docs/getting-started/installation) for platform downloads._');

  // Collapse runs of 3+ blank lines
  md = md.replace(/\n{3,}/g, '\n\n');

  return md.trim() + '\n';
}

export function buildFrontMatterHeader(title: string, description?: string): string {
  const parts = [`# ${title}`];
  if (description) parts.push(`> ${description}`);
  return parts.join('\n\n') + '\n\n';
}
