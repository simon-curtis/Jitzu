import { cn } from "@/lib/utils";
import React from "react";

const Heading1 = React.forwardRef<
  HTMLHeadingElement,
  React.HTMLAttributes<HTMLHeadingElement>
>(({ className, ...props }, ref) => {
  return (
    <h1
      className={cn("text-3xl font-bold tracking-tight text-foreground", className)}
      ref={ref}
      {...props}
    >
      {props.children}
    </h1>
  );
});
Heading1.displayName = "Heading1";

const Heading2 = React.forwardRef<
  HTMLHeadingElement,
  React.HTMLAttributes<HTMLHeadingElement>
>(({ className, ...props }, ref) => {
  return (
    <h2
      className={cn("text-2xl font-semibold tracking-tight text-foreground", className)}
      ref={ref}
      {...props}
    >
      {props.children}
    </h2>
  );
});
Heading2.displayName = "Heading2";

const Heading3 = React.forwardRef<
  HTMLHeadingElement,
  React.HTMLAttributes<HTMLHeadingElement>
>(({ className, ...props }, ref) => {
  return (
    <h3
      className={cn("text-xl font-semibold text-foreground", className)}
      ref={ref}
      {...props}
    >
      {props.children}
    </h3>
  );
});
Heading3.displayName = "Heading3";

export { Heading1, Heading2, Heading3 };
