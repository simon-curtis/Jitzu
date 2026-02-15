
// Colors from Jitzu shell's ThemeConfig.cs defaults
const theme = {
  prompt: {
    user: '#5f8787',
    directory: '#87d7ff',
    arrow: '#5faf5f',
    error: '#d75f5f',
    time: '#808080',
    ninja: '#d7af87',
  },
  git: {
    branch: '#808080',
    dirty: '#d7af87',
  },
  ls: {
    directory: '#87afd7',
    code: '#87afaf',
    config: '#d7af87',
    size: '#87af87',
    dim: '#808080',
  },
  syntax: {
    keyword: '#d7afaf',
    string: '#afaf87',
    command: '#87af87',
    flag: '#87afaf',
  },
  grep: {
    file: '#87afaf',
    match: '#d75f5f',
    lineNum: '#808080',
  },
  comment: '#808080',
  text: '#c8c8c8',
  muted: '#808080',
};

type Span = { text: string; color?: string; bold?: boolean; dim?: boolean };
type Line = Span[];

function prompt(user: string, dir: string, branch?: string, dirty?: boolean, time?: string): Line {
  const spans: Line = [
    { text: user, color: theme.prompt.user },
    { text: ' ' },
    { text: dir, color: theme.prompt.directory },
  ];
  if (branch) {
    spans.push({ text: ' ' });
    spans.push({ text: `(${branch})`, color: theme.git.branch });
    if (dirty) {
      spans.push({ text: ' *', color: theme.git.dirty });
    }
  }
  if (time) {
    spans.push({ text: `  ${time}`, color: theme.prompt.time });
  }
  return spans;
}

function arrow(): Line {
  return [{ text: '❯ ', color: theme.prompt.arrow, bold: true }];
}

function cmd(_command: string, spans: Span[]): Line {
  return [...arrow(), ...spans];
}

function comment(text: string): Line {
  return [{ text: `// ${text}`, color: theme.comment, dim: true }];
}

function lsDir(name: string, date: string): Line {
  return [
    { text: 'd-----', dim: true },
    { text: '       - ', color: theme.ls.size },
    { text: ` ${date}  `, color: theme.ls.dim },
    { text: `${name}/`, color: theme.ls.directory, bold: true },
  ];
}

function lsFile(attrs: string, size: string, date: string, name: string, type: 'code' | 'config' | 'text' = 'code'): Line {
  const colorMap = { code: theme.ls.code, config: theme.ls.config, text: theme.text };
  return [
    { text: attrs, dim: true },
    { text: `  ${size.padStart(6)}`, color: theme.ls.size },
    { text: `  ${date}  `, color: theme.ls.dim },
    { text: name, color: colorMap[type] },
  ];
}

function output(text: string, color?: string): Line {
  return [{ text, color: color || theme.text }];
}

function grepLine(file: string, lineNum: string, content: string, matchWord: string): Line {
  const before = content.substring(0, content.indexOf(matchWord));
  const after = content.substring(content.indexOf(matchWord) + matchWord.length);
  return [
    { text: file, color: theme.grep.file },
    { text: ':', color: theme.muted },
    { text: lineNum, color: theme.grep.lineNum },
    { text: ':', color: theme.muted },
    { text: before },
    { text: matchWord, color: theme.grep.match, bold: true },
    { text: after },
  ];
}

const SESSION: Line[] = [
  comment('The prompt shows user, dir, git branch & status'),
  prompt('simon@dev', '~/projects/api', 'main', true, '14:23'),
  [],
  comment('Built-in commands work out of the box'),
  cmd('ls', [{ text: 'ls', color: theme.syntax.command }]),
  lsDir('src', 'Jan 15 09:44'),
  lsDir('tests', 'Jan 10 11:20'),
  lsFile('-a-r--', '2.5K', 'Jan 15 14:23', 'Program.cs'),
  lsFile('-a-r--', '1.8K', 'Jan 12 10:05', 'README.md', 'config'),
  [],
  cmd('find src -ext cs', [
    { text: 'find', color: theme.syntax.command },
    { text: ' src ' },
    { text: '-ext', color: theme.syntax.flag },
    { text: ' cs' },
  ]),
  output('src/Parser.cs'),
  output('src/Lexer.cs'),
  output('src/Interpreter.cs'),
  [],
  comment('Pipe OS commands into Jitzu functions'),
  cmd('git log --oneline | first', [
    { text: 'git log' },
    { text: ' --oneline', color: theme.syntax.flag },
    { text: ' | ', color: theme.muted },
    { text: 'first', color: theme.syntax.command },
  ]),
  output('a1b2c3d Add pattern matching support'),
  [],
  comment('Inline Jitzu code - types, expressions, everything'),
  cmd('let greeting = "hello from the shell"', [
    { text: 'let', color: theme.syntax.keyword },
    { text: ' greeting = ' },
    { text: '"hello from the shell"', color: theme.syntax.string },
  ]),
  cmd('greeting.to_upper()', [{ text: 'greeting.to_upper()' }]),
  output('"HELLO FROM THE SHELL"', theme.syntax.string),
  [],
  comment('Aliases persist to disk automatically'),
  cmd('alias ll="ls -la"', [
    { text: 'alias', color: theme.syntax.command },
    { text: ' ll=' },
    { text: '"ls -la"', color: theme.syntax.string },
  ]),
  output('Alias set: ll → ls -la', theme.ls.dim),
  [],
  comment('Label directories for quick access'),
  cmd('label api ~/projects/api', [
    { text: 'label', color: theme.syntax.command },
    { text: ' api ~/projects/api' },
  ]),
  output('Label set: api: → /home/simon/projects/api', theme.ls.dim),
  cmd('cd api:src/', [
    { text: 'cd', color: theme.syntax.command },
    { text: ' api:src/' },
  ]),
  [],
  comment('grep with line numbers'),
  cmd('grep -rn "TODO" src/', [
    { text: 'grep', color: theme.syntax.command },
    { text: ' -rn', color: theme.syntax.flag },
    { text: ' ' },
    { text: '"TODO"', color: theme.syntax.string },
    { text: ' src/' },
  ]),
  grepLine('src/Parser.cs', '42', '    // TODO: Handle forward references', 'TODO'),
  grepLine('src/Interpreter.cs', '156', '    // TODO: Optimize stack layout', 'TODO'),
];

function RenderLine({ spans }: { spans: Line }) {
  if (spans.length === 0) return <div className="h-2" />;
  return (
    <div>
      {spans.map((span, i) => (
        <span
          key={i}
          style={{
            color: span.color,
            fontWeight: span.bold ? 'bold' : undefined,
            opacity: span.dim ? 0.6 : undefined,
          }}
        >
          {span.text}
        </span>
      ))}
    </div>
  );
}

export function ShellSession({ className = '' }: { className?: string }) {
  return (
    <div className={`relative group ${className}`}>
      <div className="relative rounded-xl overflow-hidden glass transition-shadow duration-300">
        {/* Terminal chrome header */}
        <div className="flex items-center gap-2 px-4 py-2.5 bg-white/[0.02] border-b border-white/[0.06]">
          <div className="flex items-center gap-1.5">
            <div className="w-2.5 h-2.5 rounded-full bg-white/[0.08]"></div>
            <div className="w-2.5 h-2.5 rounded-full bg-white/[0.08]"></div>
            <div className="w-2.5 h-2.5 rounded-full bg-white/[0.08]"></div>
          </div>
          <span className="text-xs text-muted-foreground ml-2">jitzu</span>
        </div>

        {/* Session content */}
        <pre
          className="p-6 overflow-auto"
          style={{
            margin: 0,
            fontFamily: 'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
            fontSize: '0.875rem',
            lineHeight: '1.6',
            color: '#c8c8c8',
            background: 'transparent',
          }}
        >
          <code style={{ fontFamily: 'inherit', fontSize: 'inherit', lineHeight: 'inherit' }}>
            {SESSION.map((line, i) => (
              <RenderLine key={i} spans={line} />
            ))}
          </code>
        </pre>
      </div>
    </div>
  );
}
