import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  Alert,
  Badge,
  Button,
  Card,
  Form,
  Heading,
  Input,
  Stack,
  Text,
} from "../index";

describe("UI Primitives", () => {
  it("Button triggers onClick and respects disabled/loading a11y state", async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();

    const { rerender } = render(
      <Button onClick={onClick} ariaLabel="Save settings">
        Save
      </Button>,
    );

    const button = screen.getByRole("button", { name: "Save settings" });
    await user.click(button);
    expect(onClick).toHaveBeenCalledTimes(1);

    rerender(
      <Button onClick={onClick} loading>
        Save
      </Button>,
    );

    expect(button).toBeDisabled();
    expect(button).toHaveAttribute("aria-busy", "true");
    await user.click(button);
    expect(onClick).toHaveBeenCalledTimes(1);
  });

  it("Input binds label, updates value, and exposes error via aria-invalid and role=alert", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();

    const { rerender } = render(
      <Input
        label="Username"
        value=""
        onChange={onChange}
        helperText="At least 3 characters"
      />,
    );

    const input = screen.getByLabelText("Username");
    expect(input).not.toHaveAttribute("aria-invalid");
    expect(input).toHaveAccessibleDescription("At least 3 characters");

    await user.type(input, "abc");
    expect(onChange).toHaveBeenCalled();

    rerender(
      <Input
        label="Username"
        value="a"
        onChange={onChange}
        error="Too short"
      />,
    );

    expect(input).toHaveAttribute("aria-invalid", "true");
    expect(screen.getByRole("alert")).toHaveTextContent("Too short");
    expect(input).toHaveAccessibleDescription("Too short");
  });

  it("Alert uses role=alert for danger/warning and role=status for info/success", () => {
    const { rerender } = render(
      <Alert tone="info" title="Notice">
        System ready
      </Alert>,
    );

    expect(screen.getByRole("status")).toHaveTextContent("Notice");
    expect(screen.getByRole("status")).toHaveTextContent("System ready");

    rerender(
      <Alert tone="danger" title="Error">
        Login failed
      </Alert>,
    );

    expect(screen.getByRole("alert")).toHaveTextContent("Login failed");
  });

  it("Heading renders semantic h1-h4 levels", () => {
    render(
      <Stack>
        <Heading level={1}>Title 1</Heading>
        <Heading level={2}>Title 2</Heading>
        <Heading level={3}>Title 3</Heading>
        <Heading level={4}>Title 4</Heading>
      </Stack>,
    );

    expect(screen.getByRole("heading", { level: 1 })).toHaveTextContent("Title 1");
    expect(screen.getByRole("heading", { level: 2 })).toHaveTextContent("Title 2");
    expect(screen.getByRole("heading", { level: 3 })).toHaveTextContent("Title 3");
    expect(screen.getByRole("heading", { level: 4 })).toHaveTextContent("Title 4");
  });

  it("Card, Stack, Text, Badge, and Form render children and submit cleanly", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn((e) => e.preventDefault());

    render(
      <Card variant="subtle" padding="md">
        <Form onSubmit={onSubmit}>
          <Stack direction="horizontal" gap="sm" align="center" justify="between">
            <Text variant="muted" as="span">
              Status:
            </Text>
            <Badge tone="success">Online</Badge>
            <Button type="submit">Submit</Button>
          </Stack>
        </Form>
      </Card>,
    );

    expect(screen.getByText("Status:")).toBeInTheDocument();
    expect(screen.getByText("Online")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Submit" }));
    expect(onSubmit).toHaveBeenCalledTimes(1);
  });
});
