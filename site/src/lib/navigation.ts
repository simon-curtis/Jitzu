export interface NavItem {
  title: string;
  url: string;
  items?: NavItem[];
}

export const navMain: NavItem[] = [
  {
    title: "Getting Started",
    url: "#",
    items: [
      { title: "Overview", url: "/docs/getting-started/overview" },
      { title: "Installation", url: "/docs/getting-started/installation" },
    ],
  },
  {
    title: "Shell",
    url: "/docs/shell",
    items: [
      { title: "Overview", url: "/docs/shell/overview" },
      { title: "File System", url: "/docs/shell/commands/file-system" },
      { title: "Text Processing", url: "/docs/shell/commands/text-processing" },
      { title: "System Info", url: "/docs/shell/commands/system-info" },
      { title: "Utilities", url: "/docs/shell/commands/utilities" },
      { title: "Session", url: "/docs/shell/commands/session" },
      { title: "Privilege Escalation", url: "/docs/shell/commands/privilege-escalation" },
      { title: "Pipes & Redirection", url: "/docs/shell/pipes-and-redirection" },
      { title: "Completion & Editing", url: "/docs/shell/completion-and-editing" },
      { title: "Customization", url: "/docs/shell/customization" },
      { title: "Activity Monitor", url: "/docs/shell/activity-monitor" },
    ],
  },
  {
    title: "Language",
    url: "/docs/language",
    items: [
      { title: "Getting Started", url: "/docs/language/getting-started" },
      { title: "Basic Syntax", url: "/docs/language/basic-syntax" },
      { title: "Data Types", url: "/docs/language/data-types" },
      { title: "Numbers", url: "/docs/language/numbers" },
      { title: "Strings", url: "/docs/language/strings" },
      { title: "Vectors", url: "/docs/language/vectors" },
      { title: "Dates", url: "/docs/language/dates" },
      { title: "Functions", url: "/docs/language/functions" },
      { title: "Control Flow", url: "/docs/language/control-flow" },
      { title: "Object-Oriented", url: "/docs/language/object-oriented" },
      { title: "Traits & Methods", url: "/docs/language/traits" },
      { title: "Pattern Matching", url: "/docs/language/pattern-matching" },
      { title: ".NET Interop", url: "/docs/language/dotnet-interop" },
      { title: "Code Style", url: "/docs/language/code-style" },
    ],
  },
];
