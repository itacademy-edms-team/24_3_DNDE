/** @jsxImportSource @emotion/react */
import { css } from '@emotion/react';
import { Link as ReactRouterLink, type LinkProps } from 'react-router';
import { forwardRef } from 'react';

// Стили для отключения дефолтной стилизации ссылок
const unstyledLinkStyles = css`
  text-decoration: none;
  color: inherit;
  &:hover,
  &:focus,
  &:active {
    text-decoration: none;
    color: inherit;
  }
`;

// Типизация пропсов для UnstyledLink
type UnstyledLinkProps = LinkProps;

const UnstyledLink = forwardRef<HTMLAnchorElement, UnstyledLinkProps>(
  ({ to, children, ...props }, ref) => {
    return (
      <ReactRouterLink to={to} css={unstyledLinkStyles} ref={ref} {...props}>
        {children}
      </ReactRouterLink>
    );
  }
);

UnstyledLink.displayName = 'UnstyledLink'; // For React DevTools

export default UnstyledLink;
