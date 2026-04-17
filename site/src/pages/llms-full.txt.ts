import type { APIRoute } from 'astro';
import { getCollection } from 'astro:content';
import { mdxToMarkdown } from '@/lib/mdxToMarkdown';

const SITE = 'https://jitzu.dev';

export const GET: APIRoute = async () => {
  const docs = await getCollection('docs');

  const bySection = new Map<string, typeof docs>();
  for (const entry of docs) {
    const section = entry.data.section ?? 'Docs';
    if (!bySection.has(section)) bySection.set(section, []);
    bySection.get(section)!.push(entry);
  }
  for (const list of bySection.values()) {
    list.sort((a, b) => (a.data.order ?? 999) - (b.data.order ?? 999));
  }

  const sectionOrder = ['Getting Started', 'Shell', 'Language'];
  const orderedSections = [
    ...sectionOrder.filter((s) => bySection.has(s)),
    ...[...bySection.keys()].filter((s) => !sectionOrder.includes(s)),
  ];

  const parts: string[] = [];
  parts.push('# Jitzu — Full Documentation\n');
  parts.push('> An interactive shell and a typed scripting language built on .NET.\n');
  parts.push(`Source: ${SITE} — all documentation pages concatenated in canonical order.\n`);

  for (const section of orderedSections) {
    parts.push(`\n---\n\n# ${section}\n`);
    for (const entry of bySection.get(section)!) {
      const slug = entry.id.replace(/\.mdx$/, '');
      parts.push(`\n---\n`);
      parts.push(`<!-- source: ${SITE}/docs/${slug} -->\n`);
      parts.push(`## ${entry.data.title}\n`);
      if (entry.data.description) parts.push(`> ${entry.data.description}\n`);
      parts.push('\n' + mdxToMarkdown(entry.body) + '\n');
    }
  }

  return new Response(parts.join('\n'), {
    headers: {
      'Content-Type': 'text/plain; charset=utf-8',
      'X-Robots-Tag': 'all',
    },
  });
};
