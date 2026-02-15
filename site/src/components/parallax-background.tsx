import { useEffect, useRef } from 'react';

interface ParallaxBackgroundProps {
  className?: string;
}

export function ParallaxBackground({ className = '' }: ParallaxBackgroundProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleScroll = () => {
      if (!containerRef.current) return;

      const scrolled = window.pageYOffset;

      const elements = containerRef.current.querySelectorAll('[data-parallax]');
      elements.forEach((element) => {
        const speed = element.getAttribute('data-parallax');

        if (speed) {
          // Simple 2D parallax without 3D transforms
          const yPos = -(scrolled * parseFloat(speed));
          (element as HTMLElement).style.transform = `translateY(${yPos}px)`;
        }
      });
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <div
      ref={containerRef}
      className={`absolute inset-0 overflow-hidden pointer-events-none ${className}`}
    >
      {/* Light mode - Subtle organic shapes */}
      <div className="dark:hidden">
        {/* Soft organic blob - far background */}
        <div
          data-parallax="0.05"
          className="absolute top-20 left-10 w-64 h-64 bg-gradient-to-br from-primary/5 to-accent/3 rounded-full blur-3xl"
        />

        {/* Gentle curve - mid background */}
        <div
          data-parallax="0.15"
          className="absolute top-40 right-20 w-48 h-48 bg-gradient-to-bl from-accent/4 to-primary/2 rounded-full blur-2xl"
        />

        {/* Subtle accent - near background */}
        <div
          data-parallax="0.25"
          className="absolute bottom-32 left-1/3 w-32 h-32 bg-gradient-to-tr from-primary/3 to-accent/2 rounded-full blur-xl"
        />
      </div>

      {/* Dark mode - Subtle night elements */}
      <div className="hidden dark:block">
        {/* Soft glow - far background */}
        <div
          data-parallax="0.05"
          className="absolute top-20 left-10 w-64 h-64 bg-gradient-to-br from-primary/8 to-accent/6 rounded-full blur-3xl"
        />

        {/* Gentle accent - mid background */}
        <div
          data-parallax="0.15"
          className="absolute top-40 right-20 w-48 h-48 bg-gradient-to-bl from-accent/6 to-primary/4 rounded-full blur-2xl"
        />

        {/* Subtle highlight - near background */}
        <div
          data-parallax="0.25"
          className="absolute bottom-32 left-1/3 w-32 h-32 bg-gradient-to-tr from-primary/5 to-accent/3 rounded-full blur-xl"
        />
      </div>

      {/* Universal subtle elements */}
      <div
        data-parallax="0.02"
        className="absolute top-1/4 left-1/4 w-40 h-40 bg-gradient-to-br from-primary/3 to-transparent rounded-full blur-2xl dark:from-primary/6"
      />

      <div
        data-parallax="0.08"
        className="absolute bottom-1/4 right-1/4 w-32 h-32 bg-gradient-to-tl from-accent/3 to-transparent rounded-xl blur-xl dark:from-accent/6"
      />

            {/* Floating particles - Higher z-index to be visible */}

            <div
                data-parallax="0.18"
                className="absolute bottom-1/3 left-1/4 z-20"
                style={{ zIndex: 20 }}
            >
                <div className="w-3 h-3 bg-accent/30 rounded-full dark:bg-accent/40 particle-float-2" />
            </div>

            {/* Additional floating particles for better visibility */}
            <div
                data-parallax="0.15"
                className="absolute top-1/2 left-[20%] z-20"
                style={{ zIndex: 20 }}
            >
                <div className="w-2 h-2 bg-primary/25 rounded-full dark:bg-primary/35 particle-float-3" />
            </div>

            <div
                data-parallax="0.22"
                className="absolute bottom-1/2 right-[20%] z-20"
                style={{ zIndex: 20 }}
            >
                <div className="w-3 h-3 bg-accent/70 rounded-full dark:bg-accent/35 particle-float-1" />
            </div>

            {/* More visible particles around the mascot area */}
            <div
                data-parallax="0.08"
                className="absolute top-1/4 right-1/3 z-20"
                style={{ zIndex: 20 }}
            >
                <div className="w-2 h-2 bg-primary/40 rounded-full dark:bg-primary/50 particle-float-2" />
            </div>

            <div
                data-parallax="0.14"
                className="absolute bottom-1/4 left-1/2 z-20"
                style={{ zIndex: 20 }}
            >
                <div className="w-2 h-2 bg-accent/70 rounded-full dark:bg-accent/50 particle-float-3" />
            </div>

      {/* Very subtle grid texture */}
      <div
        data-parallax="0.01"
        className="absolute inset-0 opacity-[0.12] dark:opacity-[0.04]"
        style={{
          backgroundImage: `
                        linear-gradient(to right, currentColor 1px, transparent 1px),
                        linear-gradient(to bottom, currentColor 1px, transparent 1px)
                    `,
          backgroundSize: '60px 60px'
        }}
      />
    </div>
  );
}
