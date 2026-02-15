import { Prism as SyntaxHighlighter } from "react-syntax-highlighter";
import { JitzuHighlighter } from "./jitzu-highlighter";

interface CodeBlockProps {
  code: string;
  language: string;
}

// Pastel theme matching Jitzu shell ThemeConfig defaults
const theme = {
  'code[class*="language-"]': {
    background: 'transparent',
    color: '#c8c8c8',
    fontFamily: 'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
  },
  'pre[class*="language-"]': {
    background: 'transparent',
    color: '#c8c8c8',
    fontFamily: 'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
  },
  comment: { color: '#808080', fontStyle: 'italic' },
  prolog: { color: '#808080' },
  doctype: { color: '#808080' },
  cdata: { color: '#808080' },
  punctuation: { color: '#a0a0a0' },
  '.token.namespace': { opacity: 0.7 },
  property: { color: '#87afaf' },
  tag: { color: '#d7afaf' },
  boolean: { color: '#d7af87' },
  number: { color: '#d7af87' },
  constant: { color: '#d7af87' },
  symbol: { color: '#d7af87' },
  deleted: { color: '#d75f5f' },
  selector: { color: '#afaf87' },
  'attr-name': { color: '#afaf87' },
  string: { color: '#afaf87' },
  char: { color: '#afaf87' },
  builtin: { color: '#afaf87' },
  inserted: { color: '#afaf87' },
  operator: { color: '#af87af' },
  entity: { color: '#a0a0a0' },
  url: { color: '#87afd7' },
  '.token.variable': { color: '#a0a0a0' },
  '.token.important': { color: '#d7afaf', fontWeight: 'bold' },
  atrule: { color: '#d7afaf' },
  'attr-value': { color: '#afaf87' },
  function: { color: '#87afd7' },
  'class-name': { color: '#87afaf' },
  keyword: { color: '#d7afaf', fontWeight: 'bold' },
  regex: { color: '#d7af87' },
  important: { color: '#af87af', fontWeight: 'bold' },
  bold: { fontWeight: 'bold' },
  italic: { fontStyle: 'italic' },
};

export function CodeBlock({ code, language }: CodeBlockProps) {
  // Use custom Jitzu highlighter for jitzu language
  if (language === 'jitzu' || language === 'jz') {
    return <JitzuHighlighter code={code} />;
  }

  return (
    <div className="relative group">
      <div className="relative rounded-xl overflow-hidden glass transition-shadow duration-300">
        {/* Header bar */}
        <div className="flex items-center px-4 py-2 bg-white/[0.02] border-b border-white/[0.06]">
          <span className="text-xs uppercase tracking-wider text-muted-foreground">
            {language}
          </span>
        </div>

        <SyntaxHighlighter
          language={language}
          style={theme}
          className="text-sm"
          customStyle={{
            margin: 0,
            padding: "1.5rem",
            backgroundColor: 'transparent',
            fontFamily: 'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
            fontSize: '0.875rem',
            lineHeight: '1.6',
          }}
        >
          {code}
        </SyntaxHighlighter>
      </div>
    </div>
  );
}
