import type { APIRoute, GetStaticPaths } from 'astro';
import { getCollection } from 'astro:content';
import { mdxToMarkdown, buildFrontMatterHeader } from '@/lib/mdxToMarkdown';

export const getStaticPaths: GetStaticPaths = async () => {
  const docs = await getCollection('docs');
  return docs.map((entry) => ({
    params: { slug: entry.id.replace(/\.mdx$/, '') },
    props: { entry },
  }));
};

export const GET: APIRoute = async ({ props }) => {
  const entry = props.entry as Awaited<ReturnType<typeof getCollection>>[number];
  const header = buildFrontMatterHeader(entry.data.title, entry.data.description);
  const body = mdxToMarkdown(entry.body);

  // If the body already starts with an H1, skip the header's H1 line
  const startsWithH1 = /^#\s+/.test(body);
  const output = startsWithH1
    ? (entry.data.description ? `> ${entry.data.description}\n\n` : '') + body
    : header + body;

  return new Response(output, {
    headers: {
      'Content-Type': 'text/markdown; charset=utf-8',
      'X-Robots-Tag': 'all',
    },
  });
};
