import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/docs/language/RouteComponent')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/docs/language/RouteComponent"!</div>
}
