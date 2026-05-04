import React from 'react';
import { type UseFormRegisterReturn } from 'react-hook-form';
import { cn } from '../../utils/cn';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helpText?: string;
  registration?: Partial<UseFormRegisterReturn>;
  leftAddon?: React.ReactNode;
  rightAddon?: React.ReactNode;
  containerClassName?: string;
}

// ─── Component ────────────────────────────────────────────────────────────────

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  (
    {
      label,
      error,
      helpText,
      registration,
      leftAddon,
      rightAddon,
      containerClassName,
      className,
      id,
      ...props
    },
    ref,
  ) => {
    const inputId = id ?? (label ? label.toLowerCase().replace(/\s+/g, '-') : undefined);
    const hasError = !!error;

    return (
      <div className={cn('flex flex-col gap-1', containerClassName)}>
        {label && (
          <label htmlFor={inputId} className="text-sm font-medium text-gray-700">
            {label}
            {props.required && <span className="ml-0.5 text-brand-danger">*</span>}
          </label>
        )}

        <div className="relative flex items-center">
          {leftAddon && (
            <div className="pointer-events-none absolute left-3 flex items-center text-gray-400">
              {leftAddon}
            </div>
          )}

          <input
            ref={ref}
            id={inputId}
            {...registration}
            {...props}
            className={cn(
              'block w-full rounded-md border bg-white text-sm text-gray-900 placeholder:text-gray-400',
              'py-2 px-3',
              'transition-colors duration-150',
              'focus:outline-none focus:ring-2 focus:ring-offset-0',
              hasError
                ? 'border-red-400 focus:border-red-400 focus:ring-red-300'
                : 'border-gray-300 focus:border-brand-primary focus:ring-blue-200',
              'disabled:bg-gray-50 disabled:text-gray-500 disabled:cursor-not-allowed',
              leftAddon && 'pl-9',
              rightAddon && 'pr-9',
              className,
            )}
          />

          {rightAddon && (
            <div className="pointer-events-none absolute right-3 flex items-center text-gray-400">
              {rightAddon}
            </div>
          )}
        </div>

        {error && (
          <p className="text-xs text-brand-danger mt-0.5" role="alert">
            {error}
          </p>
        )}
        {!error && helpText && (
          <p className="text-xs text-gray-500 mt-0.5">{helpText}</p>
        )}
      </div>
    );
  },
);

Input.displayName = 'Input';
