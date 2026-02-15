import { useMemo, useRef, useEffect, Suspense } from 'react';
import { Canvas, useFrame, useThree } from '@react-three/fiber';
import * as THREE from 'three';

interface ParticleSystemProps {
  count?: number;
  className?: string;
}

function ParticleField({ count = 150 }: { count: number }) {
  const meshRef = useRef<THREE.Points>(null);
  const mousePosition = useRef({ x: 0, y: 0 });
  const { size, viewport } = useThree();

  const particles = useMemo(() => {
    const positions = new Float32Array(count * 3);
    const colors = new Float32Array(count * 3);
    const sizes = new Float32Array(count);
    const velocities = new Float32Array(count * 3);

    const colorPalette = [
      new THREE.Color('#87afd7'), // blue
      new THREE.Color('#af87af'), // lavender
      new THREE.Color('#87afaf'), // teal
      new THREE.Color('#d7afaf'), // rose
    ];

    for (let i = 0; i < count; i++) {
      const i3 = i * 3;

      positions[i3] = (Math.random() - 0.5) * 50;
      positions[i3 + 1] = (Math.random() - 0.5) * 30;
      positions[i3 + 2] = (Math.random() - 0.5) * 20;

      const color = colorPalette[Math.floor(Math.random() * colorPalette.length)];
      colors[i3] = color.r;
      colors[i3 + 1] = color.g;
      colors[i3 + 2] = color.b;

      const sizeVariation = Math.random();
      if (sizeVariation < 0.08) {
        sizes[i] = Math.random() * 3 + 2;
      } else if (sizeVariation < 0.25) {
        sizes[i] = Math.random() * 2 + 1;
      } else {
        sizes[i] = Math.random() * 1 + 0.5;
      }

      velocities[i3] = (Math.random() - 0.5) * 0.015;
      velocities[i3 + 1] = (Math.random() - 0.5) * 0.015;
      velocities[i3 + 2] = (Math.random() - 0.5) * 0.008;
    }

    return { positions, colors, sizes, velocities };
  }, [count]);

  useEffect(() => {
    const handleMouseMove = (event: MouseEvent) => {
      mousePosition.current.x = (event.clientX / size.width) * 2 - 1;
      mousePosition.current.y = -(event.clientY / size.height) * 2 + 1;
    };

    window.addEventListener('mousemove', handleMouseMove);
    return () => window.removeEventListener('mousemove', handleMouseMove);
  }, [size]);

  useFrame((state) => {
    if (!meshRef.current) return;

    const positions = meshRef.current.geometry.attributes.position;
    const positionArray = positions.array as Float32Array;

    const time = state.clock.getElapsedTime();
    const mouse = mousePosition.current;

    for (let i = 0; i < count; i++) {
      const i3 = i * 3;

      positionArray[i3] += particles.velocities[i3] + Math.sin(time * 0.6 + i * 0.01) * 0.001;
      positionArray[i3 + 1] += particles.velocities[i3 + 1] + Math.cos(time * 0.4 + i * 0.01) * 0.001;
      positionArray[i3 + 2] += particles.velocities[i3 + 2] + Math.sin(time * 0.3 + i * 0.02) * 0.0005;

      const dx = mouse.x * viewport.width / 2 - positionArray[i3];
      const dy = mouse.y * viewport.height / 2 - positionArray[i3 + 1];
      const distance = Math.sqrt(dx * dx + dy * dy);

      if (distance < 6) {
        const force = 0.0002;
        positionArray[i3] += dx * force;
        positionArray[i3 + 1] += dy * force;
      }

      if (positionArray[i3] > 25) positionArray[i3] = -25;
      if (positionArray[i3] < -25) positionArray[i3] = 25;
      if (positionArray[i3 + 1] > 15) positionArray[i3 + 1] = -15;
      if (positionArray[i3 + 1] < -15) positionArray[i3 + 1] = 15;
      if (positionArray[i3 + 2] > 10) positionArray[i3 + 2] = -10;
      if (positionArray[i3 + 2] < -10) positionArray[i3 + 2] = 10;
    }

    positions.needsUpdate = true;

    meshRef.current.rotation.y += 0.0004;
    meshRef.current.rotation.x = Math.sin(time * 0.08) * 0.015;
  });

  return (
    <points ref={meshRef}>
      <bufferGeometry>
        <bufferAttribute
          attach="attributes-position"
          args={[particles.positions, 3]}
        />
        <bufferAttribute
          attach="attributes-color"
          args={[particles.colors, 3]}
        />
        <bufferAttribute
          attach="attributes-size"
          args={[particles.sizes, 1]}
        />
      </bufferGeometry>
      <pointsMaterial
        size={0.05}
        sizeAttenuation={true}
        vertexColors={true}
        transparent={true}
        opacity={0.5}
        blending={THREE.AdditiveBlending}
      />
    </points>
  );
}

export function ThreeParticleSystem({ count = 150, className = '' }: ParticleSystemProps) {
  return (
    <div className={`absolute inset-0 pointer-events-none ${className}`}>
      <Canvas
        camera={{ position: [0, 0, 5], fov: 75 }}
        style={{ background: 'transparent' }}
        dpr={Math.min(window.devicePixelRatio, 2)}
      >
        <Suspense fallback={null}>
          <ParticleField count={count} />
        </Suspense>
      </Canvas>
    </div>
  );
}
