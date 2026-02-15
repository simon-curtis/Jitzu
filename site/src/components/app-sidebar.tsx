import type * as React from "react";
import { Minus, Plus } from "lucide-react";

import { SearchForm } from "@/components/search-form";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarRail,
} from "@/components/ui/sidebar";
import { Link } from "@tanstack/react-router";

type NavigationItem = {
  title: string,
  url: string,
  isActive?: boolean,
  items?: NavigationItem[]
}

const navMain: NavigationItem[] = [
  {
    title: "Getting Started",
    url: "#",
    items: [
      {
        title: "Installation",
        url: "/docs/getting-started/installation",
      },
    ],
  },
  {
    title: "Language",
    url: "/docs/language",
    items: [
      {
        title: "Getting Started",
        url: "/docs/language/getting-started",
      },
      {
        title: "Basic Syntax",
        url: "/docs/language/basic-syntax",
      },
      {
        title: "Data Types",
        url: "/docs/language/data-types",
      },
      {
        title: "Numbers",
        url: "/docs/language/numbers",
      },
      {
        title: "Strings",
        url: "/docs/language/strings",
      },
      {
        title: "Vectors",
        url: "/docs/language/vectors",
      },
      {
        title: "Dates",
        url: "/docs/language/dates",
      },
      {
        title: "Functions",
        url: "/docs/language/functions",
      },
      {
        title: "Control Flow",
        url: "/docs/language/control-flow",
      },
      {
        title: "Object-Oriented",
        url: "/docs/language/object-oriented",
      },
      {
        title: "Pattern Matching",
        url: "/docs/language/pattern-matching",
      },
    ],
  },
  {
    title: "Shell",
    url: "/docs/shell",
    items: [
      {
        title: "Overview",
        url: "/docs/shell/overview",
      },
      {
        title: "Commands",
        url: "/docs/shell/commands",
      },
      {
        title: "Pipes & Redirection",
        url: "/docs/shell/pipes-and-redirection",
      },
      {
        title: "Completion & Editing",
        url: "/docs/shell/completion-and-editing",
      },
      {
        title: "Customization",
        url: "/docs/shell/customization",
      },
      {
        title: "Activity Monitor",
        url: "/docs/shell/activity-monitor",
      },
    ],
  },
  {
    title: "Community",
    url: "#",
    items: [
      {
        title: "Contribution Guide",
        url: "#",
      },
    ],
  },
];

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  return (
    <Sidebar {...props}>
      <SidebarHeader>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton size="lg" asChild>
              <Link to="/">
                <div className="flex flex-col gap-0.5 leading-none">
                  <span className="text-gradient-primary font-bold text-lg">Jitzu</span>
                  <span className="text-xs text-muted-foreground">Documentation</span>
                </div>
              </Link>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
        <SearchForm />
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarMenu>
            {navMain.map((item, index) => (
              <Collapsible
                key={item.title}
                defaultOpen={index === 1}
                className="group/collapsible"
              >
                <SidebarMenuItem>
                  <CollapsibleTrigger asChild>
                    <SidebarMenuButton className="text-muted-foreground text-xs uppercase tracking-wider font-medium">
                      {item.title}{" "}
                      <Plus className="ml-auto group-data-[state=open]/collapsible:hidden" />
                      <Minus className="ml-auto group-data-[state=closed]/collapsible:hidden" />
                    </SidebarMenuButton>
                  </CollapsibleTrigger>
                  {item.items?.length ? (
                    <CollapsibleContent>
                      <SidebarMenuSub>
                        {item.items.map((item) => (
                          <SidebarMenuSubItem key={item.title}>
                            <SidebarMenuSubButton
                              asChild
                              isActive={item.isActive === true}
                            >
                              <Link to={item.url}>{item.title}</Link>
                            </SidebarMenuSubButton>
                          </SidebarMenuSubItem>
                        ))}
                      </SidebarMenuSub>
                    </CollapsibleContent>
                  ) : null}
                </SidebarMenuItem>
              </Collapsible>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>
      <SidebarRail />
    </Sidebar>
  );
}
