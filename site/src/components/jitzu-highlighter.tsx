
interface JitzuHighlighterProps {
  code: string;
  className?: string;
  showLineNumbers?: boolean;
}

interface Token {
  type: 'keyword' | 'string' | 'number' | 'boolean' | 'function' | 'type' | 'comment' | 'operator' | 'property' | 'text';
  value: string;
}

function tokenizeJitzu(code: string): Token[] {
  const tokens: Token[] = [];
  let remaining = code;
  let position = 0;

  const patterns = [
    // Strings - template literals, double quotes, single quotes
    { type: 'string' as const, regex: /^`[^`]*`/ },
    { type: 'string' as const, regex: /^"(?:[^"\\]|\\[\s\S])*"/ },
    { type: 'string' as const, regex: /^'(?:[^'\\]|\\[\s\S])*'/ },

    // Numbers
    { type: 'number' as const, regex: /^0x[\da-f]+/i },
    { type: 'number' as const, regex: /^0b[01]+/ },
    { type: 'number' as const, regex: /^\d+\.?\d*(?:e[+-]?\d+)?[fl]?/i },

    // Booleans
    { type: 'boolean' as const, regex: /^(true|false)\b/ },

    // Keywords
    { type: 'keyword' as const, regex: /^(let|mut|const|fun|return|if|else|match|case|for|while|in|try|catch|finally|defer|async|await|yield|break|continue|enum|trait|impl|type|open|import|package|static|pub|public|private|internal|override|abstract|virtual|sealed|partial|where|when|is|as|new|delete|this|super|null|and|or|not|xor|mod|div|assert|like|Result|Ok|Err)\b/ },

    // Function calls
    { type: 'function' as const, regex: /^[a-zA-Z_]\w*(?=\s*\()/ },

    // Types (capital letters)
    { type: 'type' as const, regex: /^[A-Z][a-zA-Z0-9_]*\b/ },

    // Property access
    { type: 'property' as const, regex: /^\.([a-zA-Z_]\w*)/ },

    // Operators (excluding braces for now)
    { type: 'operator' as const, regex: /^(\+\+|--|==|!=|<=|>=|&&|\|\||->|=>|\.\.|\.\.\.|\?\?|[+\-*/%=!<>&|^~?:.])+/ },

    // Identifiers and text
    { type: 'text' as const, regex: /^[a-zA-Z_]\w*/ },
    { type: 'text' as const, regex: /^\s+/ },
    { type: 'text' as const, regex: /^[^\n]/ },
    { type: 'text' as const, regex: /^\n/ }
  ];

  while (remaining.length > 0) {
    let matched = false;

    // Handle comments first (must be before operator matching)
    if (remaining.startsWith('//')) {
      const newlineIndex = remaining.indexOf('\n');
      const commentText = newlineIndex === -1 ? remaining : remaining.slice(0, newlineIndex);
      tokens.push({
        type: 'comment',
        value: commentText
      });
      remaining = remaining.slice(commentText.length);
      position += commentText.length;
      matched = true;
    } else if (remaining.startsWith('/*')) {
      const endIndex = remaining.indexOf('*/');
      if (endIndex !== -1) {
        const commentText = remaining.slice(0, endIndex + 2);
        tokens.push({
          type: 'comment',
          value: commentText
        });
        remaining = remaining.slice(commentText.length);
        position += commentText.length;
        matched = true;
      }
    }

    // Explicit single quote handling
    if (!matched && remaining.startsWith("'")) {
      let endQuote = 1;
      let escaped = false;

      while (endQuote < remaining.length) {
        const char = remaining[endQuote];
        if (escaped) {
          escaped = false;
        } else if (char === '\\') {
          escaped = true;
        } else if (char === "'") {
          // Found closing quote
          tokens.push({
            type: 'string',
            value: remaining.slice(0, endQuote + 1)
          });
          remaining = remaining.slice(endQuote + 1);
          position += endQuote + 1;
          matched = true;
          break;
        }
        endQuote++;
      }

      // If no closing quote found, treat as text
      if (!matched) {
        tokens.push({ type: 'text', value: remaining[0] });
        remaining = remaining.slice(1);
        position += 1;
        matched = true;
      }
    }

    // Special handling for template literals with interpolation
    if (!matched) {
      const templateMatch = remaining.match(/^`([^`]*)`/);
      if (templateMatch) {
        const content = templateMatch[1];

        // Handle template literal with potential interpolation
        if (content.includes('{') && content.includes('}')) {
          tokens.push({ type: 'string', value: '`' });

          let templateRemaining = content;
          while (templateRemaining.length > 0) {
            const braceStart = templateRemaining.indexOf('{');
            if (braceStart === -1) {
              // No more interpolation, rest is string
              tokens.push({ type: 'string', value: templateRemaining });
              break;
            }

            // Add string part before brace
            if (braceStart > 0) {
              tokens.push({ type: 'string', value: templateRemaining.slice(0, braceStart) });
            }

            // Find matching closing brace
            const braceEnd = templateRemaining.indexOf('}', braceStart);
            if (braceEnd === -1) {
              // No closing brace, treat as string
              tokens.push({ type: 'string', value: templateRemaining });
              break;
            }

            // Add opening brace as operator
            tokens.push({ type: 'operator', value: '{' });

            // Tokenize interpolated content properly
            const interpolated = templateRemaining.slice(braceStart + 1, braceEnd);

            // Simple identifier (like 'name' or 'self.name')
            if (interpolated.match(/^[a-zA-Z_]\w*(\.[a-zA-Z_]\w*)*$/)) {
              const parts = interpolated.split('.');
              for (let i = 0; i < parts.length; i++) {
                if (i > 0) tokens.push({ type: 'operator', value: '.' });
                // Check if it's a type (starts with capital)
                if (parts[i].match(/^[A-Z]/)) {
                  tokens.push({ type: 'type', value: parts[i] });
                } else {
                  tokens.push({ type: 'text', value: parts[i] });
                }
              }
            } else {
              // More complex expression, try to identify basic patterns
              let exprRemaining = interpolated;
              while (exprRemaining) {
                // Try to match keywords first
                const keywordMatch = exprRemaining.match(/^(let|mut|const|fun|return|if|else|match|case|for|while|in|try|catch|finally|defer|async|await|yield|break|continue|enum|trait|impl|type|open|import|package|static|pub|public|private|internal|override|abstract|virtual|sealed|partial|where|when|is|as|new|delete|this|super|null|and|or|not|xor|mod|div|assert|like|Result|Ok|Err)\b/);
                if (keywordMatch) {
                  tokens.push({ type: 'keyword', value: keywordMatch[0] });
                  exprRemaining = exprRemaining.slice(keywordMatch[0].length);
                  continue;
                }

                // Try to match identifiers
                const identMatch = exprRemaining.match(/^[a-zA-Z_]\w*/);
                if (identMatch) {
                  if (identMatch[0].match(/^[A-Z]/)) {
                    tokens.push({ type: 'type', value: identMatch[0] });
                  } else {
                    tokens.push({ type: 'text', value: identMatch[0] });
                  }
                  exprRemaining = exprRemaining.slice(identMatch[0].length);
                  continue;
                }

                // Match operators and other symbols
                const opMatch = exprRemaining.match(/^(\+\+|--|==|!=|<=|>=|&&|\|\||->|=>|\.\.|\.\.\.|\?\?|[+\-*/%=!<>&|^~?:.])+/);
                if (opMatch) {
                  tokens.push({ type: 'operator', value: opMatch[0] });
                  exprRemaining = exprRemaining.slice(opMatch[0].length);
                  continue;
                }

                // Match whitespace
                const spaceMatch = exprRemaining.match(/^\s+/);
                if (spaceMatch) {
                  tokens.push({ type: 'text', value: spaceMatch[0] });
                  exprRemaining = exprRemaining.slice(spaceMatch[0].length);
                  continue;
                }

                // Fallback: consume one character
                tokens.push({ type: 'text', value: exprRemaining[0] });
                exprRemaining = exprRemaining.slice(1);
              }
            }

            // Add closing brace as operator
            tokens.push({ type: 'operator', value: '}' });

            templateRemaining = templateRemaining.slice(braceEnd + 1);
          }

          tokens.push({ type: 'string', value: '`' });
        } else {
          // Simple template literal without interpolation
          tokens.push({ type: 'string', value: templateMatch[0] });
        }

        remaining = remaining.slice(templateMatch[0].length);
        position += templateMatch[0].length;
        matched = true;
      }
    }

    if (!matched) {
      for (const pattern of patterns) {
        // Skip template literal pattern since we handled it above
        if (pattern.regex.source.includes('`')) continue;

        const match = remaining.match(pattern.regex);
        if (match) {
          tokens.push({
            type: pattern.type,
            value: match[0]
          });
          remaining = remaining.slice(match[0].length);
          position += match[0].length;
          matched = true;
          break;
        }
      }
    }

    if (!matched) {
      // Fallback: consume one character
      tokens.push({
        type: 'text',
        value: remaining[0]
      });
      remaining = remaining.slice(1);
      position += 1;
    }
  }

  return tokens;
}

export function JitzuHighlighter({
  code,
  className = '',
  showLineNumbers = false
}: JitzuHighlighterProps) {
  const tokens = tokenizeJitzu(code);
  const lines = code.split('\n');

  // Pastel color scheme matching Jitzu shell ThemeConfig defaults
  const colors = {
    keyword: '#d7afaf',  // Rose pink
    string: '#afaf87',   // Warm olive
    number: '#d7af87',   // Amber
    boolean: '#d7af87',  // Amber
    function: '#87afd7', // Soft blue
    type: '#87afaf',     // Teal
    comment: '#808080',  // Gray
    operator: '#af87af', // Lavender
    property: '#87afaf', // Teal
    text: '#c8c8c8',     // Light gray
  };

  // Convert tokens to JSX
  const renderTokens = () => {
    return tokens.map((token, index) => {
      const style = {
        color: colors[token.type],
        fontWeight: token.type === 'keyword' ? 'bold' : 'normal',
        fontStyle: token.type === 'comment' ? 'italic' : 'normal'
      };

      return (
        <span key={index} style={style}>
          {token.value}
        </span>
      );
    });
  };

  return (
    <div className={`relative group ${className}`}>
      <div className="relative rounded-xl overflow-hidden glass transition-shadow duration-300">
        {/* Header bar */}
        <div className="flex items-center px-4 py-2 bg-white/[0.02] border-b border-white/[0.06]">
          <span className="text-xs uppercase tracking-wider text-muted-foreground">
            Jitzu
          </span>
        </div>

        {/* Code content */}
        <div className="flex">
          {/* Line numbers */}
          {showLineNumbers && (
            <div
              className="px-4 py-6 text-right select-none border-r border-white/[0.06]"
              style={{
                color: '#666',
                fontFamily: 'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
                fontSize: '0.875rem',
                lineHeight: '1.6',
                backgroundColor: 'rgba(255,255,255,0.05)'
              }}
            >
              {lines.map((_, index) => (
                <div key={`line-${index + 1}`}>
                  {(index + 1).toString().padStart(2, ' ')}
                </div>
              ))}
            </div>
          )}

          {/* Code */}
          <pre
            className="flex-1 p-6 overflow-auto"
            style={{
              margin: 0,
              fontFamily: 'JetBrains Mono, Consolas, Monaco, "Andale Mono", monospace',
              fontSize: '0.875rem',
              lineHeight: '1.6',
              color: colors.text,
              background: 'transparent'
            }}
          >
            <code
              style={{
                fontFamily: 'inherit',
                fontSize: 'inherit',
                lineHeight: 'inherit'
              }}
            >
              {renderTokens()}
            </code>
          </pre>
        </div>
      </div>
    </div>
  );
}
