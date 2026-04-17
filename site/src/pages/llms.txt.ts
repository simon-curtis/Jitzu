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
  lines.push('> A typed shell and scripting language on .NET. Pipe OS command output into typed functions; call any NuGet package directly.');
  lines.push('');
  lines.push('Jitzu ships as a single binary (`jz`). Status: alpha — small stdlib, not production-ready. Open source.');
  lines.push('');
  lines.push('## At a glance');
  lines.push('');
  lines.push('```jitzu');
  lines.push('// Typed pipe: OS command output → Jitzu function');
  lines.push('ls -ext cs | map(f => f.name.ToUpper()) | first(3)');
  lines.push('');
  lines.push('// Result<T, E> with the ? operator');
  lines.push('fun load_config(): Result<Config, Error> {');
  lines.push('    let file = try read_file("config.json")');
  lines.push('    let parsed = try parse_json(file)');
  lines.push('    return Ok(parsed)');
  lines.push('}');
  lines.push('');
  lines.push('// Union types + pattern matching');
  lines.push('let area = match shape {');
  lines.push('    Shape.Circle(r) => 3.14159 * r ** 2,');
  lines.push('    Shape.Square(s) => s ** 2,');
  lines.push('}');
  lines.push('');
  lines.push('// NuGet, inline');
  lines.push('#:package Newtonsoft.Json@13.0.4');
  lines.push('open "Newtonsoft.Json" as { JsonConvert }');
  lines.push('let json = JsonConvert.SerializeObject(user)');
  lines.push('```');
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
