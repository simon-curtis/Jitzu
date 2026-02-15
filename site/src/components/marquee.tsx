import type { CSSProperties, PropsWithChildren } from "react";

interface MarqueeProps extends PropsWithChildren {
  speed?: number;
  pauseOnHover?: boolean;
  className?: string;
  reverse?: boolean;
}

export function Marquee({
  children,
  speed = 40,
  pauseOnHover = true,
  className = "",
  reverse = false,
}: MarqueeProps) {
  const style = {
    "--marquee-duration": `${speed}s`,
    "--marquee-direction": reverse ? "reverse" : "normal",
  } as CSSProperties;

  return (
    <div
      className={`overflow-hidden ${className}`}
      style={style}
    >
      <div
        className={`flex w-max gap-8 ${pauseOnHover ? "hover:[animation-play-state:paused]" : ""}`}
        style={{
          animation: `marquee var(--marquee-duration) linear infinite var(--marquee-direction)`,
        }}
      >
        {children}
        {/* Duplicate for seamless loop */}
        {children}
      </div>
    </div>
  );
}
