import { useEffect, useRef } from 'react';
import { ThreeParticleSystem } from './three-particle-system';

interface EnhancedParallaxBackgroundProps {
  className?: string;
  enableParticles?: boolean;
  particleCount?: number;
}

export function EnhancedParallaxBackground({
  className = '',
  enableParticles = true,
  particleCount = 150
}: EnhancedParallaxBackgroundProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleScroll = () => {
      if (!containerRef.current) return;

      const scrolled = window.pageYOffset;

      const elements = containerRef.current.querySelectorAll('[data-parallax]');
      elements.forEach((element) => {
        const speed = element.getAttribute('data-parallax');

        if (speed) {
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
      {/* Three.js Particle System */}
      {enableParticles && (
        <div className="absolute inset-0 z-10">
          <ThreeParticleSystem count={particleCount} />
        </div>
      )}

      {/* Subtle grid */}
      <div
        data-parallax="0.01"
        className="absolute inset-0 opacity-[0.02]"
        style={{
          backgroundImage: `
            linear-gradient(to right, #1e1e2e 1px, transparent 1px),
            linear-gradient(to bottom, #1e1e2e 1px, transparent 1px)
          `,
          backgroundSize: '60px 60px'
        }}
      />
    </div>
  );
}
