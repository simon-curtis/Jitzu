import { useMotionValue, useSpring, motion } from "framer-motion";
import { useEffect } from "react";

export function CursorGlow() {
  const mouseX = useMotionValue(-500);
  const mouseY = useMotionValue(-500);

  const springX = useSpring(mouseX, { stiffness: 150, damping: 20 });
  const springY = useSpring(mouseY, { stiffness: 150, damping: 20 });

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      mouseX.set(e.clientX);
      mouseY.set(e.clientY);
    };

    window.addEventListener("mousemove", handleMouseMove);
    return () => window.removeEventListener("mousemove", handleMouseMove);
  }, [mouseX, mouseY]);

  return (
    <motion.div
      className="pointer-events-none fixed z-0"
      style={{
        x: springX,
        y: springY,
        width: 400,
        height: 400,
        marginLeft: -200,
        marginTop: -200,
        background:
          "radial-gradient(circle, color-mix(in srgb, var(--pastel-blue) 7%, transparent) 0%, color-mix(in srgb, var(--pastel-lavender) 4%, transparent) 40%, transparent 70%)",
        borderRadius: "50%",
        filter: "blur(40px)",
      }}
    />
  );
}
