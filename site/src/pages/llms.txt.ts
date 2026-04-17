import type { APIRoute } from 'astro';
import { getCollection } from 'astro:content';

const SITE = 'https://jitzu.dev';

export const GET: APIRoute = async () => {
  const docs = await getCollection('docs');

  // Group by section, preserve order field
  const bySection = new Map<string, typeof docs>();
  for (const entry of docs) {
    const section = entry.data.section ?? 'Docs';
    if (!bySection.has(section)) bySection.set(section, []);
    bySection.get(section)!.push(entry);
  }
  for (const list of bySection.values()) {
    list.sort((a, b) => (a.data.order ?? 999) - (b.data.order ?? 999));
  }

  const lines: string[] = [];
  lines.push('# Jitzu');
  lines.push('');
  lines.push('> An interactive shell and a typed scripting language built on .NET. Jitzu combines a modern terminal replacement with a scripting language that shares the same runtime, type system, and package ecosystem.');
  lines.push('');
  lines.push('Jitzu ships as a single binary (`jz`). The shell replaces bash/zsh/PowerShell; the language is a statically-typed scripting language with .NET interop, pattern matching, and traits.');
  lines.push('');

  const sectionOrder = ['Getting Started', 'Shell', 'Language'];
  const orderedSections = [
    ...sectionOrder.filter((s) => bySection.has(s)),
    ...[...bySection.keys()].filter((s) => !sectionOrder.includes(s)),
  ];

  for (const section of orderedSections) {
    lines.push(`## ${section}`);
    lines.push('');
    for (const entry of bySection.get(section)!) {
      const slug = entry.id.replace(/\.mdx$/, '');
      const url = `${SITE}/docs/${slug}.md`;
      const desc = entry.data.description ? `: ${entry.data.description}` : '';
      lines.push(`- [${entry.data.title}](${url})${desc}`);
    }
    lines.push('');
  }

  lines.push('## Optional');
  lines.push('');
  lines.push(`- [Full documentation (single file)](${SITE}/llms-full.txt): Every documentation page concatenated into a single markdown file for one-shot ingestion.`);
  lines.push(`- [GitHub repository](https://github.com/jitzulang/jitzu): Source code, issues, releases.`);
  lines.push('');

  return new Response(lines.join('\n'), {
    headers: {
      'Content-Type': 'text/plain; charset=utf-8',
      'X-Robots-Tag': 'all',
    },
  });
};
