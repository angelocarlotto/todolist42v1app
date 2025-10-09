# Speckit Constitution: Principles for todolist42v1app

## 1. Code Quality
- All code must be readable, maintainable, and follow established style guides (Airbnb for JavaScript/React, MongoDB best practices).
- Use meaningful variable, function, and component names.
- Avoid code duplication; prefer modular, reusable React components and MongoDB schema definitions.
- Document all public functions, classes, components, and database models with clear comments or docstrings.
- Use PropTypes or TypeScript for React component type safety.
- Validate MongoDB schemas and sanitize all database inputs.
- Perform regular code reviews to ensure adherence to standards.

## 2. Testing Standards (with TDD)
- Practice Test-Driven Development (TDD): write tests before implementing new features or bug fixes.
- All features and bug fixes must include automated tests (unit, integration, and end-to-end as appropriate).
- Achieve and maintain at least 80% code coverage for both frontend (React) and backend (MongoDB-related logic).
- Use Jest, React Testing Library, and Supertest (or similar) for testing React components and API endpoints.
- Mock MongoDB connections in tests to ensure isolation and speed.
- Use descriptive test names and clear assertions.
- Run the full test suite before merging any code changes.

## 3. User Experience Consistency
- Maintain a consistent look and feel across all React UI components (colors, fonts, spacing, and interactions).
- Ensure accessibility (WCAG 2.1 AA compliance) for all user-facing features.
- Provide clear feedback for user actions (loading states, errors, confirmations).
- Minimize user friction by streamlining workflows and reducing unnecessary steps.
- Support responsive design for all device sizes.
- Ensure all data interactions with MongoDB are reflected in the UI in real time or with clear refresh cues.

## 4. Performance Requirements
- Optimize for fast load times (target <2s for initial load on broadband).
- Minimize React bundle size and use code splitting/lazy loading where possible.
- Use React memoization (React.memo, useMemo, useCallback) to avoid unnecessary re-renders.
- Use MongoDB indexes and efficient queries to optimize database performance.
- Avoid blocking the main thread; use asynchronous operations for I/O and heavy computations.
- Monitor and address performance regressions regularly (use Lighthouse, React Profiler, MongoDB monitoring tools).
- Profile and optimize critical user flows for speed and efficiency.

---

These principles are mandatory for all contributors and must be reviewed and updated as the project evolves.